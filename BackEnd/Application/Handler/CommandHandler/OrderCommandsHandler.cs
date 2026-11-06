using Application.Commands;
using Application.Common;
using Application.Dtos;
using Application.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;



    public class OrderCommandHandler(
            IOrderRepository _repo,
            IOrderItemRepository _itemRepo,
            IBackgroundJobService _jobService,
            IHttpContextAccessor _accessor,
            IDiscountCodeRepository _dcRepo,
            ICartRepository _cartRepo) :
        IRequestHandler<CheckoutCartCommand, ServiceResult<OrderIdDto>>,
        IRequestHandler<DeleteOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteOrderItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveOrderItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<AddOrderItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<RemoveOrderItemCommand, ServiceResult<IdDto>>,
        IRequestHandler<ConfirmOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<PayOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<CancelOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<ShipOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeliverOrderCommand, ServiceResult<IdDto>>,
        IRequestHandler<ApplyDiscountCodeCommand, ServiceResult<IdDto>>,
        IRequestHandler<RemoveDiscountCodeCommand, ServiceResult<IdDto>>
    {


    public async Task<ServiceResult<OrderIdDto>> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId(); if (userId == null) return ServiceResult<OrderIdDto>.Unauthorized();
        DiscountCode? discountCode = null;
        var DiscountAmount = 0m;
        var totalAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.DiscountCode)) { discountCode = await _dcRepo.Query(dc => dc.Code == request.DiscountCode).FirstOrDefaultAsync();
            if (discountCode == null) return ServiceResult<OrderIdDto>.Failed("کد تخفیف پیدا نشد");
            var now = DateTime.UtcNow;
            if (!(discountCode.StartDate <= now && discountCode.EndDate >= now)) return ServiceResult<OrderIdDto>.Failed("کد تخفیف اعتبار زمانی ندارد");
            if (!discountCode.IsActive) return ServiceResult<OrderIdDto>.Failed("کد تخفیف فعال نیست");
            if (!discountCode.IsValid) return ServiceResult<OrderIdDto>.Failed("کد تخفیف معتبر نیست");
            if (discountCode.UsedCount >= discountCode.UsageLimit) return ServiceResult<OrderIdDto>.Failed("تعداد دفعات استفاده از کد تخفیف به اتمام رسیده");
            discountCode.Use(userId.Value); }
        var cart = await _cartRepo.GetUserCartAsync(userId.Value);
        foreach (var item in cart.Items) { 
            var bestDiscount = item.ProductOffer.Discounts.Where(pd => !pd.IsDeleted && pd.ProductOfferId == item.ProductOfferId).OrderByDescending(pd => pd.Discount.Priority).FirstOrDefault();
            decimal discountValue = 0;
            if (bestDiscount != null) { 
                discountValue = bestDiscount.Discount.IsPercent ? item.ProductOffer.BasePrice * bestDiscount.Discount.Amount / 100m : bestDiscount.Discount.Amount; }
            DiscountAmount += discountValue * item.Quantity;
            totalAmount += (item.ProductOffer.BasePrice * item.Quantity) - (discountValue * item.Quantity);
        } 
        if (discountCode != null) { decimal codeDiscount = discountCode.IsPercent ? totalAmount * discountCode.Amount / 100m : discountCode.Amount; DiscountAmount += codeDiscount; } 
        var orderId = await _repo.CreateOrderFromCartAsync( userId.Value, request.ShippingAddressId, request.ShippingMethodId, request.PaymentMethodId, request.ShippingCost ?? 0, DiscountAmount, discountCode?.Id );
        if (orderId == 0) return ServiceResult<OrderIdDto>.Failed("سفارش ثبت نشد؛ کالایی برای ثبت سفارش در سبد خرید شما پیدا نشد.");
        _jobService.Schedule<IOrderJobService>( job => job.DeleteOrderAfter15Min(orderId, userId.Value), TimeSpan.FromMinutes(15));
        return ServiceResult<OrderIdDto>.Ok(new OrderIdDto { OrderId = orderId });
    }
 
        public async Task<ServiceResult<IdDto>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetByIdAsync(request.Id);
            if (order == null)
                return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            order.Delete(userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteOrderItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var order = await _itemRepo.GetByIdAsync(request.Id);
        if (order == null)
            return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
        order.Delete(userId.Value);
        await _itemRepo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
    }
        public async Task<ServiceResult<IdDto>> Handle(ActiveOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var order = await _repo.GetByIdAsync(request.Id);
        if (order == null)
            return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");

        order.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
    }
        public async Task<ServiceResult<IdDto>> Handle(ActiveOrderItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var orderItem = await _itemRepo.GetByIdAsync(request.Id);
        if (orderItem == null)
            return ServiceResult<IdDto>.Failed("این کالا در سفارش پیدا نشد.");

        orderItem.SetActive(!orderItem.IsActive, userId.Value);
        await _itemRepo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = orderItem.Id });
    }
        public async Task<ServiceResult<IdDto>> Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var result = await _repo.AddItemToOrderAsync(request.OrderId, request.ProductOfferId, request.Quantity, userId.Value);
            if (result == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = result.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetOrderWithItemsAsync(request.OrderId);
            if (order == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");

            order.RemoveItem(request.ProductId, userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetOrderWithItemsAsync(request.OrderId);
            if (order == null)
                return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");

            order.Confirm(userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(PayOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");

            order.Pay(userId.Value);
            var cart = await _cartRepo.GetUserCartAsync(userId.Value);
            await _cartRepo.ClearCartAsync(userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            order.Cancel(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            order.Ship(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });

        }
        public async Task<ServiceResult<IdDto>> Handle(DeliverOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null) return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            order.Deliver(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ApplyDiscountCodeCommand request, CancellationToken cancellationToken)
        {
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null)
                return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");

            var discount = await _dcRepo.GetByIdAsync(request.DiscountCodeId);
            if (discount == null || !discount.IsValid)
                return ServiceResult<IdDto>.Failed("کد تخفیف معتبر نیست؛ ممکن است منقضی شده یا ظرفیت آن تمام شده باشد.");

            // صدا زدن متد درست و ارسال currentUserId
            order.ApplyDiscountCode(discount, request.CurrentUserId);
            discount.Use(request.CurrentUserId);

            await _repo.SaveChangesAsync(cancellationToken);
            await _dcRepo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(RemoveDiscountCodeCommand request, CancellationToken cancellationToken)
        {
            var order = await _repo.GetByIdAsync(request.OrderId);
            if (order == null)
                return ServiceResult<IdDto>.Failed("سفارش موردنظر پیدا نشد.");
            order.RemoveDiscountCode(request.CurrentUserId);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = order.Id });
        }
    }  
 