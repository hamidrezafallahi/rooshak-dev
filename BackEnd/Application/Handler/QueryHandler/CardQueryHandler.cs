using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class CartQueryHandler(ICartRepository _cartRepo, IHttpContextAccessor _accessor, IEntityConfigRepository _configRepo) : 
        IRequestHandler<GetAllCartsQuery, ServiceResult<ListDto<CartDto>>>,
        IRequestHandler<GetCartByIdQuery, ServiceResult<CartDto?>>,
        IRequestHandler<GetDetailCartByIdQuery, ServiceResult<detailCartDto?>>,
        IRequestHandler<GetCartsByUserIdQuery, ServiceResult<CartDto>>
    {
        public async Task<ServiceResult<ListDto<CartDto>>> Handle(GetAllCartsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            IQueryable<Cart> query;
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _cartRepo.Query().Include(c => c.Items).ThenInclude(i=>i.ProductOffer);
            }
            else {
            query = _cartRepo.Query(b => b.IsActive).Include(c=>c.Items).ThenInclude(i => i.ProductOffer);
            }
            int totalCount =await query.CountAsync(cancellationToken);
            var pagedCartItems = await query
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);
            
            var now = DateTime.UtcNow;

            var CartItemsDto = pagedCartItems.Select(c => new CartDto
            {
                Id = c.Id,
                UserId = c.UserId,
                IsActive=c.IsActive,
                ItemsCount = c.Items.Count(z => !z.IsDeleted),  
                TotalPrice = c.Items
        .Where(z => !z.IsDeleted)
        .Sum(i => i.Quantity * i.ProductOffer.GetFinalPrice(now))
            }).ToList();

            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("carts");
            }

            var resultDto = new ListDto<CartDto>
            {
                Records = CartItemsDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<CartDto>>.Ok(resultDto);

 
        }
        public async Task<ServiceResult<CartDto?>> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartRepo.GetByIdWithItemsAsync(request.Id);

            if (cart == null)
                return ServiceResult<CartDto?>.Failed("سبد خرید پیدا نشد.");

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Status=cart.Status,
                Items = cart.Items
                .Where(z => !z.IsDeleted)
                .Select(i => new CartItemDto
                {
                    ProductOfferId = i.ProductOfferId,
                    Quantity = i.Quantity,
                    Id = i.Id,
                    CartId = i.CartId
                }).ToList()
            };

            return ServiceResult<CartDto?>.Ok(cartDto);
        }
        public async Task<ServiceResult<detailCartDto?>> Handle(GetDetailCartByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<detailCartDto?>.Unauthorized();
            var cart = await _cartRepo.GetDetailCartAsync(request.Id, userId.Value);
            if (cart == null)
                return ServiceResult<detailCartDto?>.Failed("سبد خرید پیدا نشد.");
            var detailCart = new detailCartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new DetailCartItemDto
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.BasePrice,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Discounts = i.Discounts.Select(d => new ItemDiscountDto
                    {
                        IsPercent = d.IsPercent,
                        Amount = d.Amount,
                        AmountAfterCalc = d.IsPercent
                                    ? i.BasePrice - (i.BasePrice * (d.Amount / 100))
                                    : i.BasePrice - d.Amount
                    }).ToList()
                }).ToList()

            };


            return ServiceResult<detailCartDto?>.Ok(detailCart);
        }
        public async Task<ServiceResult<CartDto>> Handle(GetCartsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<CartDto>.Unauthorized();
            var cart = await _cartRepo.GetByUserIdWithItemsAsync(userId.Value);

            if (cart == null || !cart.Items.Any(i => !i.IsDeleted))
                return ServiceResult<CartDto>.Failed("سبد خرید فعالی پیدا نشد.");

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items
                .Where(z => !z.IsDeleted)
                .Select(i => new CartItemDto
                {
                    ProductOfferId = i.ProductOfferId,
                    Quantity = i.Quantity,
                    Id = i.Id,
                    CartId = i.CartId,

                }).ToList()
            };

            return ServiceResult<CartDto?>.Ok(cartDto);
        }

    }
    public class CartItemQueryHandler(ICartItemRepository _repo, IEntityConfigRepository _configRepo) :
        IRequestHandler<GetAllCartItemsQuery, ServiceResult<ListDto<CartItemDto>>>,
        IRequestHandler<GetCartItemByIdQuery, ServiceResult<CartItemDto?>>,
        IRequestHandler<GetCartItemsByCartIdQuery, ServiceResult<IEnumerable<CartItemDto>>>
    {
        public async Task<ServiceResult<ListDto<CartItemDto>>> Handle(GetAllCartItemsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            IQueryable<CartItem> query;
            if (request.Q is not null && request.Q.Length > 0)
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {
                    query = _repo.Query()
                        .Include(ci => ci.ProductOffer)
                        .Include(ci=>ci.Cart)
                            .ThenInclude(c=>c.User)
                        .Include(ci=>ci.Product)
                            .ThenInclude(p => p.Images)
                        .Where(ci=>ci.Product.Name.Contains(request.Q));

                }
                else
                {
                    query = _repo.Query(ci => ci.IsActive).Include(ci => ci.ProductOffer)
                        .Include(ci => ci.Cart)
                            .ThenInclude(c => c.User)
                        .Include(ci => ci.Product)
                            .ThenInclude(p => p.Images).Where(ci => ci.Product.Name.Contains(request.Q));
                }
            }
            else
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {

                    query = _repo.Query().Include(ci => ci.ProductOffer)
                        .Include(ci => ci.Cart)
                            .ThenInclude(c => c.User)
                        .Include(ci => ci.Product)
                            .ThenInclude(p => p.Images);
                }
                else
                {
                    query = _repo.Query(b => b.IsActive)                     
                        .Include(ci => ci.ProductOffer)
                        .Include(ci => ci.Cart)
                            .ThenInclude(c => c.User)
                        .Include(ci => ci.Product)
                            .ThenInclude(p => p.Images);
                }
            }

            int totalCount = await query.CountAsync(cancellationToken);
            var pagedCartItems = await query
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);
            var CartItemsDto = pagedCartItems.Select(CI => new CartItemDto
            {
                Id = CI.Id,
                IsActive=CI.IsActive,
                CartId = CI.CartId,
                ProductId=CI.ProductId,
                UserName= CI.Cart.User.FullName,
                ProductOfferId = CI.ProductOfferId,
                ProductName=CI.Product.Name,
                ProductImage = CI.Product.Images.Where(i => i.IsMain && !i.IsDeleted).Select(i => i.ImageUrl).First(),
                Quantity = CI.Quantity,
                BasePrice=CI.UnitPrice,
                FinalPrice=CI.TotalPrice
            }).ToList();

            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("cartItems");
            }

            var resultDto = new ListDto<CartItemDto>
            {
                Records = CartItemsDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<CartItemDto>>.Ok(resultDto);
        }
        public async Task<ServiceResult<CartItemDto?>> Handle(GetCartItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repo.GetByIdAsync(request.Id);
            if (item == null)
                return ServiceResult<CartItemDto?>.Failed("این کالا در سبد خرید پیدا نشد.");

            var itemDto = new CartItemDto
            {
                Id = item.Id,
                CartId = item.CartId,
                ProductOfferId = item.ProductOfferId,
                ProductId=item.ProductId,
                Quantity = item.Quantity,
            };

            return ServiceResult<CartItemDto?>.Ok(itemDto);
        }
        public async Task<ServiceResult<IEnumerable<CartItemDto>>> Handle(GetCartItemsByCartIdQuery request, CancellationToken cancellationToken)
        {
            var items = await _repo.GetItemsByCartIdAsync(request.Id);

            var itemsDto = items.Select(item => new CartItemDto
            {
                Id = item.Id,
                CartId = item.CartId,
                ProductOfferId = item.ProductOfferId,
                Quantity = item.Quantity
            });

            return ServiceResult<IEnumerable<CartItemDto>>.Ok(itemsDto);
        }
    }
}


