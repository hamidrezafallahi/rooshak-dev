using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Repositories;

namespace OnlineShop.Infrastructure.Persistence.Repositories
{
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context)
        {
        }

        // متدهای اختصاصی CartItem 👇
        public async Task<IEnumerable<CartItem>> GetItemsByCartIdAsync(int cartId)
        {
            return await Query(ci => ci.CartId == cartId)
                 .Include(ci => ci.ProductOffer)
                 .ToListAsync();
        }

        public async Task<CartItem?> GetItemByOfferAndCartAsync(int cartId, int productOfferId)
        {
            return await _context.CartItems
        .Include(ci => ci.ProductOffer)
            .ThenInclude(o => o.Product)
        .FirstOrDefaultAsync(ci =>
            ci.CartId == cartId &&
            ci.ProductOfferId == productOfferId);
        }

        public async Task<bool> ExistsAsync(int cartId, int productOfferId)
        {
            return await _context.CartItems
         .AnyAsync(ci => ci.CartId == cartId && ci.ProductOfferId == productOfferId);
        }

        //public async Task<CartItem> UpdateQuantityAsync(int cartItemId, int quantity, int currentUserId)
        //{
        //    var item = await Query(ci => ci.Id == cartItemId).FirstOrDefaultAsync();
        //    if (item != null)
        //    {
        //        item.Update(quantity, currentUserId);
        //        await _context.SaveChangesAsync();

        //    }
        //    return item;
        //}

        public async Task<bool> RemoveAllByCartIdAsync(int cartId,int currentUserId)
        {
            var items = await Query(ci => ci.CartId == cartId && !ci.IsDeleted).ToListAsync();
            if (!items.Any())
                return false;
            items.ForEach(i => i.Delete(currentUserId));
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<decimal> GetTotalAmountByCartIdAsync(int cartId)
        {
            return await Query(ci => ci.CartId == cartId)
                .SumAsync(ci =>
    (
        ci.ProductOffer.BasePrice
        -
        ci.ProductOffer.Discounts
            .Where(d => !d.IsDeleted && d.Discount.EndDate > DateTime.Now)
            .OrderByDescending(d => d.Discount.Priority)
            .Select(d => d.Discount.IsPercent
                ? ci.ProductOffer.BasePrice * d.Discount.Amount / 100m
                : d.Discount.Amount)
            .FirstOrDefault()
    )
    * ci.Quantity
);


        }

        public async Task<CartReadModel?> GetUserCartSummaryAsync(int userId)
        {
            var cart = await _context.Carts
                .Where(c => !c.IsDeleted && c.UserId == userId)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductOffer)
                        .ThenInclude(o => o.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductOffer)
                        .ThenInclude(o => o.Product)
                            .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync();

            if (cart == null) return null;

            var readModel = new CartReadModel
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items
                    .Where(i => !i.IsDeleted)
                    .Select(i =>
                    {
                        var offer = i.ProductOffer;
                        var product = offer.Product;

                        var mainImage = product.Images
                            .Where(img => !img.IsDeleted && img.IsMain)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault();
                        var discountAmount = offer.Discounts
                            .Where(d =>
                                !d.IsDeleted &&
                                d.Discount.StartDate < DateTime.Now &&
                               DateTime.Now<d.Discount.EndDate  )
                            .OrderByDescending(d => d.Discount.Priority)  
                            .Select(d => d.Discount.IsPercent
                                ? offer.BasePrice * d.Discount.Amount / 100m
                                : d.Discount.Amount)
                            .FirstOrDefault();

                        return new CartItemReadModel
                        {
                            CartItemId=i.Id,
                            ProductId = product.Id,
                            ProductOfferId = offer.Id,
                            Name = ProductOffers.BuildDisplayName(product.Name, offer.VariantName, offer.VariantValue),
                            BasePrice = offer.BasePrice,
                            Quantity = i.Quantity,
                            Description = product.Description,
                            DiscountAmount = discountAmount,
                            FinalPrice = offer.BasePrice - discountAmount,
                            MainImage = mainImage
                        };
                    })
                    .ToList()
            };

            readModel.TotalDiscount = readModel.Items.Sum(i => i.DiscountAmount * i.Quantity);
            readModel.TotalPrice = readModel.Items.Sum(i => i.BasePrice * i.Quantity);

            return readModel;
        }




    }
}
