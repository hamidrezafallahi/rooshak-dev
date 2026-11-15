using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context) { }

    public async Task<DetailCartReadModel?> GetDetailCartAsync(int userId, int cartId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
                .ThenInclude(ci => ci.ProductOffer)
                    .ThenInclude(p => p.Discounts)
                        .ThenInclude(pd => pd.Discount)
             .Include(c => c.Items.Where(i => !i.IsDeleted))
                        .ThenInclude(p => p.ProductOffer)
                                   .ThenInclude(po => po.Product)
                    .ThenInclude(po => po.Images)
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.UserId == userId && c.Id == cartId);

        if (cart == null) return null;

        var readModel = new DetailCartReadModel
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = cart.Items.Select(i =>
            {
                var offer = i.ProductOffer;
                var product = offer.Product;

                var mainImage = product.Images.FirstOrDefault(img => img.IsMain && !img.IsDeleted)?.ImageUrl;

                var discount = offer.Discounts
                    .Where(pd => !pd.IsDeleted && pd.Discount.EndDate > DateTime.Now)
                    .OrderByDescending(pd => pd.Discount.Priority)
                    .Select(pd => pd.Discount.IsPercent
                        ? offer.BasePrice * pd.Discount.Amount / 100
                        : pd.Discount.Amount)
                    .FirstOrDefault();

                return new DetailCartItemReadModel
                {
                    ProductId = product.Id,
                    ProductOfferId = offer.Id,
                    Name = ProductOffers.BuildDisplayName(product.Name, offer.VariantName, offer.VariantValue),
                    BasePrice = offer.BasePrice,
                    Quantity = i.Quantity,
                    Description = product.Description,
                    DiscountAmount = discount,
                    FinalPrice = (offer.BasePrice - discount) * i.Quantity,
                    MainImage = mainImage
                };
            }).ToList()
        };

        return readModel;
    }

    public async Task<Cart?> GetUserCartAsync(int userId)
    {
        return await Query(c => c.UserId == userId && !c.IsDeleted)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .ThenInclude(i => i.ProductOffer)
            .ThenInclude(p => p.Discounts)
            .ThenInclude(pd => pd.Discount)
            .FirstOrDefaultAsync();
    }


    public async Task<Cart?> GetByIdWithItemsAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<Cart?> GetByUserIdWithItemsAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }
    public async Task<int> ClearCartAsync(int currentUserId)
    {
        var cart = await _context.Carts
     .Include(c => c.Items)
     .FirstOrDefaultAsync(c => c.UserId == currentUserId && !c.IsDeleted);

        if (cart == null || !cart.Items.Any())
            return 0;
        // ===== بارگذاری آدرس ارسال =====
        foreach (var item in cart.Items.Where(i => !i.IsDeleted))
        {
            item.Delete(currentUserId);
        }
        await _context.SaveChangesAsync();
        return 1;
    }

}
