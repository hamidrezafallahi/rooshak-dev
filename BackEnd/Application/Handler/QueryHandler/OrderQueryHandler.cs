using Application.Common;
using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

 public class OrderQueryHandler(IOrderRepository _repository,IOrderItemRepository _itemsRepo, IEntityConfigRepository _configRepo, IHttpContextAccessor _accessor) :
    IRequestHandler<GetOpenOrdersQuery, ServiceResult<IEnumerable<OrderDto>>>,
    IRequestHandler<GetOrdersByUserIdQuery, ServiceResult<IEnumerable<OrderReadModel>>>,
    IRequestHandler<GetOrderWithItemsQuery, ServiceResult<OrderReadModel?>>
{

    public async Task<ServiceResult<IEnumerable<OrderDto>>> Handle(GetOpenOrdersQuery request, CancellationToken cancellationToken)
        {
        var orders = await _repository.Query()
            .Include(o => o.Items)
                .ThenInclude(i => i.ProductOffer)
                    .ThenInclude(po => po.Product)
            .Include(o => o.ShippingAddress)
            .ToListAsync();


            var ordersDto = orders.Select(or => new OrderDto
            {
                Id = or.Id,
                OrderDate = or.OrderDate,
                Status = or.Status,
                TotalPrice = or.TotalPrice,
                UserId = or.UserId,
                Items=or.Items.Select(oi=>new OrderItemDto {
                OrderId = oi.Id,
                ProductOfferId = oi.ProductOfferId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Product= new ProductReadModel
                {
                    Name=oi.ProductOffer?.Product?.Name ?? "",
                    Description=oi.ProductOffer?.Product?.Description,
                    Price=oi.ProductOffer?.BasePrice ?? 0,
                }
                }).ToList(),
            }).ToList(); 
            return ServiceResult<IEnumerable<OrderDto>>.Ok(ordersDto);
        }
    public async Task<ServiceResult<IEnumerable<OrderReadModel>>> Handle(GetOrdersByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IEnumerable<OrderReadModel>>.Failed("Unauthorized");
        var orders = await _repository.GetOrdersByUserIdAsync(userId.Value);
        return ServiceResult<IEnumerable<OrderReadModel>>.Ok(orders);
    }
    public async Task<ServiceResult<OrderReadModel?>> Handle(GetOrderWithItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<OrderReadModel>.Failed("Unauthorized");

        var order = await _repository.GetOrderWithItemsReadModelAsync(request.OrderId);
        if (order == null)
            return ServiceResult<OrderReadModel?>.Failed("Order not found");
        var dto = new OrderReadModel
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            DiscountCode = order.DiscountCode,
            DiscountPrice = order.DiscountPrice,
            ShippingMethod = new ShippingMethodReadModel { Title = order.ShippingMethod.Title, Cost = order.ShippingMethod.Cost },
            PaymentMethod = new PaymentMethodReadModel { Title = order.PaymentMethod.Title },
            ShippingAddress = new UserAddressReadModel { Name = order.ShippingAddress.Name, FullAddress = order.ShippingAddress.FullAddress },
            Items = order.Items.Select(it => new OrderItemReadModel
            {
                Product = new ProductReadModel
                {
                    Name = it.Product.Name,
                    Description = it.Product.Description,
                    Price = it.Product.Price,
                    Image = it.Product.Image
                },
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice
            }).ToList()
        };

        return ServiceResult<OrderReadModel?>.Ok(dto);
    }

}
 
 public class OrderItemsQueryHandler(IOrderItemRepository _repository, IEntityConfigRepository _configRepo, IHttpContextAccessor _accessor) :
    IRequestHandler<GetOrderItemsQuery, ServiceResult<ListDto<DisplayOrderItemDto>>>,
    IRequestHandler<GetItemsByOrderIdQuery, ServiceResult<IEnumerable<OrderItemReadModel?>>>,
    IRequestHandler<GetOrderItemByIdQuery, ServiceResult<OrderItemReadModel?>>,
    IRequestHandler<CalculateTotalPriceQuery, ServiceResult<TotalPriceDto>>
{
    public async Task<ServiceResult<ListDto<DisplayOrderItemDto>>> Handle(GetOrderItemsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        string filter = request.Q;
        IQueryable<OrderItem> query;

        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repository.Query()
                    .Include(oi => oi.ProductOffer)
                        .ThenInclude(po => po.Product).ThenInclude(p => p.Images)
                    .Include(oi => oi.Order)
                        .ThenInclude(or => or.User)
                    .Where(oi => oi.ProductOffer.Product.Name.Contains(request.Q) ||
                    oi.Order.User.FullName.Contains(request.Q) ||
                    oi.ProductOffer.Supplier.FullName.Contains(request.Q))
                    .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
            }
            else
            {
                query = _repository.Query(b => b.IsActive)
                     .Include(oi => oi.ProductOffer)
                        .ThenInclude(po => po.Product).ThenInclude(p => p.Images)
                    .Include(oi => oi.Order)
                        .ThenInclude(or => or.User)
                    .Where(oi => oi.ProductOffer.Product.Name.Contains(request.Q) ||
                    oi.Order.User.FullName.Contains(request.Q) ||
                    oi.ProductOffer.Supplier.FullName.Contains(request.Q))
                    .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repository.Query().Include(oi => oi.ProductOffer)
                        .ThenInclude(po => po.Product).ThenInclude(p=>p.Images)
                    .Include(oi => oi.Order)
                        .ThenInclude(or => or.User)
                        .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
            }
            else
            {
                query = _repository.Query(b => b.IsActive).Include(oi => oi.ProductOffer)
                        .ThenInclude(po => po.Product).ThenInclude(p => p.Images)
                    .Include(oi => oi.Order)
                        .ThenInclude(or => or.User).Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
            }
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedOrderItems = await query
            //.Skip((pageNumber - 1) * pageSize)
            //.Take(pageSize)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var orderItemsDto = pagedOrderItems.Select(oi => {
            var productImageUrl = oi.ProductOffer.Product.Images
                .Where(i => i.IsMain && !i.IsDeleted)
                .Select(i => i.ImageUrl)
                .FirstOrDefault();
            var supplierImageUrl = oi.ProductOffer.Supplier.Image;
     
            return new DisplayOrderItemDto
            {
                Id = oi.Id,
                User = oi.Order.User.FullName,
                ProductOfferUser = supplierImageUrl,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.UnitPrice* oi.Quantity,
                ProductName = oi.ProductOffer.Product.Name,
                ProductImage = productImageUrl,
                OrderStatus = oi.Order.Status.OrderStatusToDisplay(),
                IsActive=oi.IsActive
            };
        }).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("orderItems");
        }
        var resultDto = new ListDto<DisplayOrderItemDto>
        {
            Records = orderItemsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<DisplayOrderItemDto>>.Ok(resultDto);
    }

    public async Task<ServiceResult<IEnumerable<OrderItemReadModel?>>> Handle( GetItemsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            var orderItems = await _repository.GetItemsByOrderIdAsync(request.OrderId);

            if (orderItems == null || !orderItems.Any())
                return ServiceResult<IEnumerable<OrderItemReadModel?>>.Failed("Order not found");

            return ServiceResult<IEnumerable<OrderItemReadModel?>>.Ok(orderItems);
        }
    public async Task<ServiceResult<OrderItemReadModel?>> Handle(GetOrderItemByIdQuery request,CancellationToken cancellationToken)
    {
        var orderItem = _repository.Query(oi => oi.Id == request.Id).FirstOrDefault();

        if (orderItem == null)
            return ServiceResult<OrderItemReadModel?>.Failed("Order not found");
        var orderItemDto = new OrderItemReadModel
        {
            Id = orderItem.Id,
            OrderId=orderItem.OrderId,
            ProductOfferId = orderItem.ProductOfferId,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice
        };


        return ServiceResult<OrderItemReadModel?>.Ok(orderItemDto);
    }
    public async Task<ServiceResult<TotalPriceDto>> Handle(CalculateTotalPriceQuery request, CancellationToken cancellationToken)
    {
        var total = await _repository.CalculateTotalPriceAsync(request.OrderId);
        if (total == null) return ServiceResult<TotalPriceDto>.Failed("total price not found");
        return ServiceResult<TotalPriceDto>.Ok(new TotalPriceDto { TotalPrice = total });
    }
}
 
