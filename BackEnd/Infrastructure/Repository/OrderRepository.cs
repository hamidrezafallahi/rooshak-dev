using OnlineShop.Domain.Common;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Enums;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<OrderReadModel>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductOffer)
                        .ThenInclude(po => po.Product)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .ToListAsync();

            return orders.Select(o => new OrderReadModel
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                ShippingAddress = new UserAddressReadModel
                {
                    Id = o.ShippingAddress.Id,
                    City = o.ShippingAddress.City,
                    State = o.ShippingAddress.State,
                    PostalCode = o.ShippingAddress.PostalCode,
                    FullAddress = o.ShippingAddress.FullAddress
                },
                Items = o.Items.Select(i => new OrderItemReadModel
                {
                    ProductOfferId = i.ProductOfferId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Product = new ProductReadModel
                    {
                        Name = i.ProductOffer.Product.Name,
                        Description = i.ProductOffer.Product.Description
                    }
                }).ToList()
            });
        }

        public async Task<UserAddress?> GetUserAddressByIdAsync(int addressId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted);
        }

        public async Task<OrderReadModel?> GetOrderWithItemsReadModelAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductOffer)
                        .ThenInclude(po => po.Product)
                            .ThenInclude(p => p.Images)
                .Include(o => o.ShippingAddress)
                .Include(o => o.PaymentMethod)
                .Include(o => o.ShippingMethod)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null) return null;

            return new OrderReadModel
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                DiscountCode = order.DiscountCode?.Code,
                DiscountPrice = order.DiscountAmount,
                PaymentMethod = new PaymentMethodReadModel { Title = order.PaymentMethod.Title },
                ShippingMethod = new ShippingMethodReadModel
                {
                    Title = order.ShippingMethod.Title,
                    Cost = order.ShippingMethod.Price
                },
                ShippingAddress = new UserAddressReadModel
                {
                    Id = order.ShippingAddress.Id,
                    Name = order.ShippingAddress.Name,
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.State,
                    PostalCode = order.ShippingAddress.PostalCode,
                    FullAddress = order.ShippingAddress.FullAddress
                },
                Items = order.Items.Select(i => new OrderItemReadModel
                {
                    ProductOfferId = i.ProductOfferId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Product = new ProductReadModel
                    {
                        Name = i.ProductOffer.Product.Name,
                        Image = i.ProductOffer.Product.Images.FirstOrDefault(img => img.IsMain)?.ImageUrl,
                        Description = i.ProductOffer.Product.Description
                    }
                }).ToList()
            };
        }

        public async Task<Order?> GetOrderWithItemsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.ProductOffer)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);
        }



        public async Task<OrderReadModel?> AddItemToOrderAsync(
            int orderId,
            int productOfferId,
            int quantity,
            int currentUserId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null) return null;

            var offer = await _context.ProductOffers
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == productOfferId && po.IsActive);

            if (offer == null) return null;

            order.AddItem(offer, quantity, currentUserId);
            await _context.SaveChangesAsync();

            return await GetOrderWithItemsReadModelAsync(order.Id);
        }




        public async Task<bool> DeleteOrderByUserIdAsJobAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId &&
                    !o.IsDeleted &&
                    (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Confirmed));

            if (order == null) return false;
            order.DeleteOrder(userId);
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
            if (orderItems == null) return false;
            foreach (var item in orderItems)
            {
                var offer = await _context.ProductOffers.FirstOrDefaultAsync(o => o.Id == item.ProductOfferId);
                offer.increaseInventory(item.Quantity, userId);
                item.Delete(userId);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateOrderFromCartAsync(
            int currentUserId,
            int shippingAddressId,
            int shippingMethodId,
            int paymentMethodId,
            decimal shippingCost,
            decimal discountAmount,
            int? discountCodeId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductOffer)
                .FirstOrDefaultAsync(c => c.UserId == currentUserId && !c.IsDeleted);

            if (cart == null || !cart.Items.Any()) return 0;

            var order = Order.Create(
                currentUserId,
                shippingAddressId,
                shippingMethodId,
                paymentMethodId,
                shippingCost,
                discountAmount,
                discountCodeId);

            foreach (var item in cart.Items.Where(i => !i.IsDeleted))
            {
                item.ProductOffer.DecreaseInventory(item.Quantity, currentUserId);
                order.AddItem(item.ProductOffer, item.Quantity, currentUserId);
                item.Delete(currentUserId);
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order.Id;
        }
        public async Task<bool> UpdateOrderDetailsAsync(
            int orderId,
            int? shippingAddressId,
            int? shippingMethodId,
            int? paymentMethodId,
            decimal? shippingCost,
            decimal? discountAmount,
            string? trackingCode,
            int currentUserId)
        {
            var order = await GetOrderWithItemsAsync(orderId);
            if (order == null) return false;

            order.Update(
                shippingAddressId,
                shippingMethodId,
                paymentMethodId,
                shippingCost,
                discountAmount,
                trackingCode,
                currentUserId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status, int currentUserId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null) return false;

            switch (status)
            {
                case OrderStatus.Confirmed: order.Confirm(currentUserId); break;
                case OrderStatus.Paid: order.Pay(currentUserId); break;
                case OrderStatus.Shipped: order.Ship(currentUserId); break;
                case OrderStatus.Delivered: order.Deliver(currentUserId); break;
                case OrderStatus.Cancelled: order.Cancel(currentUserId); break;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkOrderAsPaidAsync(int orderId, int currentUserId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Paid, currentUserId);
        }

        public async Task<bool> SetTrackingCodeAsync(int orderId, string trackingCode, int currentUserId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null) return false;

            order.SetTrackingCode(trackingCode, currentUserId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApplyDiscountCodeAsync(int orderId, int discountCodeId, int currentUserId)
        {
            var order = await GetOrderWithItemsAsync(orderId);
            if (order == null) return false;

            var code = await _context.DiscountCode
                .FirstOrDefaultAsync(c => c.Id == discountCodeId && c.IsValid && !c.IsDeleted);

            if (code == null) throw new DomainException("کد تخفیف پیدا نشد یا معتبر نیست.");

            order.ApplyDiscountCode(code, currentUserId);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveDiscountCodeAsync(int orderId, int currentUserId)
        {
            var order = await GetOrderWithItemsAsync(orderId);
            if (order == null) return false;

            order.RemoveDiscountCode(currentUserId);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
