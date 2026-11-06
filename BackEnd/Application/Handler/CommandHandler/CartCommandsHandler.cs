using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

    public class CartCommandHandler(ICartRepository _repo, ICartItemRepository _itemRepo, IProductOfferRepository _offerRepo, IHttpContextAccessor _accessor) : 
        IRequestHandler<UpdateCartCommand, ServiceResult<IdDto>>,
        IRequestHandler<AddOrUpdateCartItemCommand, ServiceResult<CartReadModel>>,
        IRequestHandler<UpdateCartItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<DecreaseCartItemCommand, ServiceResult<CartReadModel>>,
        IRequestHandler<ActiveCartItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteCartItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<SyncCartCommand, ServiceResult<CartReadModel>>
    {
        public async Task<ServiceResult<IdDto>> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Unauthorized();

            var cart = await _repo.GetByIdAsync(request.Id);
            if (cart == null) return ServiceResult<IdDto>.Failed("سبد خرید یافت نشد");

            var now = DateTime.UtcNow;
            var cartItems = new List<CartItem>();

            foreach (var i in request.Items)
            {
                var offer = await _offerRepo.GetByIdAsync(i.ProductOfferId);   // ← i.ProductOfferId
                if (offer == null) continue;

                var unitPrice = offer.GetFinalPrice(now);
                var item = CartItem.Create(
                    cart.Id,
                    i.ProductId,
                    i.ProductOfferId,      // ← تغییر
                    unitPrice,
                    i.Quantity,
                    userId.Value
                );
                cartItems.Add(item);
            }

            cart.UpdateItems(cartItems, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = cart.Id });
        }
        public async Task<ServiceResult<CartReadModel>> Handle(AddOrUpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<CartReadModel>.Unauthorized();

            var cart = await _repo.GetUserCartAsync(userId.Value);
            if (cart == null)
            {
                cart = Cart.Create(userId.Value, new List<CartItem>(), userId.Value);
                await _repo.AddAsync(cart);
                await _repo.SaveChangesAsync(cancellationToken);
            }

            var now = DateTime.UtcNow;
            var offer = await _offerRepo.GetByIdAsync(request.ProductOfferId);
            if (offer == null) return ServiceResult<CartReadModel>.Failed("پیشنهاد محصول یافت نشد");

            var unitPrice = offer.BasePrice;

            var existingItem = cart.Items.FirstOrDefault(i => !i.IsDeleted && i.ProductId == request.ProductId && i.ProductOfferId == request.ProductOfferId) ?? null;

            if (existingItem == null)
            {
                var newItem = CartItem.Create(cart.Id, request.ProductId, request.ProductOfferId, unitPrice, request.Quantity, userId.Value);
                await _itemRepo.AddAsync(newItem);
            }
            else
            {
                existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity, userId.Value);
            }

            await _itemRepo.SaveChangesAsync(cancellationToken);
            var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);
            if (updatedCart != null)
            {
                foreach (var item in updatedCart.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.MainImage))
                        item.MainImage = item.MainImage;
                }
            }

            return ServiceResult<CartReadModel>.Ok(updatedCart);
        }
    public async Task<ServiceResult<IdDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
       
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();
        var cart = await _repo.GetUserCartAsync(userId.Value);
        if (cart == null) return ServiceResult<IdDto>.Failed("سبد یافت نشد");
        var existingItem = cart.Items.FirstOrDefault(i => !i.IsDeleted && i.ProductId == request.ProductId && i.ProductOfferId == request.ProductOfferId) ?? null;
        if (existingItem == null) return ServiceResult<IdDto>.Failed("این کالا در سبد خرید شما پیدا نشد.");

        existingItem.Update(request.ProductId,
      request.ProductOfferId,
      request.Quantity,
      userId.Value);

         await _itemRepo.SaveChangesAsync(cancellationToken);

        var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);

        if (updatedCart != null)
        {
            foreach (var it in updatedCart.Items)
            {
                if (!string.IsNullOrWhiteSpace(it.MainImage))
                    it.MainImage = it.MainImage;
            }
        }

        return ServiceResult<IdDto>.Ok(new IdDto {Id= updatedCart.Id });
    }

    public async Task<ServiceResult<CartReadModel>> Handle(DecreaseCartItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<CartReadModel>.Unauthorized();
            var cart = await _repo.GetUserCartAsync(userId.Value);
            if (cart == null) return ServiceResult<CartReadModel>.Failed("سبد یافت نشد");



            var existingItem = cart.Items.FirstOrDefault(i => !i.IsDeleted && i.ProductId == request.ProductId && i.ProductOfferId == request.ProductOfferId) ?? null;
            if (existingItem == null) return ServiceResult<CartReadModel>.Failed("این کالا در سبد خرید شما پیدا نشد.");

            if (existingItem.Quantity > 1)
                existingItem.UpdateQuantity(existingItem.Quantity - 1, userId.Value);
            else
                existingItem.Delete(userId.Value);

            await _itemRepo.SaveChangesAsync(cancellationToken);

            var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);

            if (updatedCart != null)
            {
                foreach (var it in updatedCart.Items)
                {
                    if (!string.IsNullOrWhiteSpace(it.MainImage))
                        it.MainImage = it.MainImage;
                }
            }

            return ServiceResult<CartReadModel>.Ok(updatedCart);
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
        {
 
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Unauthorized();

            var cartItem = await _itemRepo.GetByIdAsync(request.Id);
            if (cartItem == null) return ServiceResult<IdDto>.Failed("آیتم سبد پیدا نشد.");

            cartItem.Delete(userId.Value);
            await _itemRepo.SaveChangesAsync(cancellationToken);

            var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);
            if (updatedCart != null)
            {
                foreach (var item in updatedCart.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.MainImage))
                        item.MainImage = item.MainImage;
                }
            }

            return ServiceResult<IdDto>.Ok(new IdDto { Id = updatedCart.Id });
        }
    public async Task<ServiceResult<IdDto>> Handle(ActiveCartItemCommand request, CancellationToken cancellationToken)
    {

        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var cartItem = await _itemRepo.GetByIdAsync(request.Id);
        if (cartItem == null) return ServiceResult<IdDto>.Failed("آیتم سبد پیدا نشد.");

        cartItem.SetActive(request.IsActive, userId.Value);
        await _itemRepo.SaveChangesAsync(cancellationToken);

        var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);
        if (updatedCart != null)
        {
            foreach (var item in updatedCart.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.MainImage))
                    item.MainImage = item.MainImage;
            }
        }

        return ServiceResult<IdDto>.Ok(new IdDto {Id=updatedCart.Id });
    }

    public async Task<ServiceResult<CartReadModel>> Handle(SyncCartCommand request, CancellationToken cancellationToken)
        {

            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<CartReadModel>.Unauthorized();

            var userCart = await _repo.GetUserCartAsync(userId.Value) ?? Cart.Create(userId.Value, new List<CartItem>(), userId.Value);
            if (userCart.Id == 0) await _repo.AddAsync(userCart);

            var now = DateTime.UtcNow;

            foreach (var clientItem in request.ClientItems)
            {
                var offer = await _offerRepo.GetByIdAsync(clientItem.ProductOfferId);
                if (offer == null) continue;

                var unitPrice = offer.BasePrice;

                var existing = userCart.Items.FirstOrDefault(i => i.ProductOfferId == clientItem.ProductOfferId && !i.IsDeleted);

                if (existing != null)
                {
                    existing.UpdateQuantity(clientItem.Quantity, userId.Value);
                }
                else
                {
                    var newItem = CartItem.Create(userCart.Id, clientItem.ProductId, clientItem.ProductOfferId, unitPrice, clientItem.Quantity, userId.Value);
                    await _itemRepo.AddAsync(newItem);
                }
            }

            await _itemRepo.SaveChangesAsync(cancellationToken);

            var updatedCart = await _itemRepo.GetUserCartSummaryAsync(userId.Value);
            if (updatedCart != null)
            {
                foreach (var item in updatedCart.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.MainImage))
                        item.MainImage = item.MainImage;
                }
            }

            return ServiceResult<CartReadModel>.Ok(updatedCart);
        }
    }