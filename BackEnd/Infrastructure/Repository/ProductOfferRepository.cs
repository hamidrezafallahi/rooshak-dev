using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductOfferRepository : Repository<ProductOffers>, IProductOfferRepository
    {
        private readonly AppDbContext _context;

        public ProductOfferRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }



        public async Task<ProductOffers?> GetByIdWithDetailsAsync(int offerId)
        {
            return await _context.ProductOffers
                .Include(o => o.Product)
                .Include(o => o.Supplier)
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDeleted);
        }



        public async Task<ProductOffers?> GetByProductAndSupplierAsync(int productId, int supplierId)
        {
            return await _context.ProductOffers
                .FirstOrDefaultAsync(o =>
                    o.ProductId == productId &&
                    o.SupplierId == supplierId &&
                    !o.IsDeleted);
        }

        /// <summary>
        /// یک تأمین‌کننده برای هر تنوع (مثلاً هر بزرگنمایی) فقط یک پیشنهاد فعال دارد.
        /// پیشنهاد بدون تنوع با پیشنهاد بدون تنوع دیگر تکراری حساب می‌شود.
        /// </summary>
        public async Task<bool> ExistsAsync(int productId, int supplierId, string? variantValue = null)
        {
            var value = string.IsNullOrWhiteSpace(variantValue) ? null : variantValue.Trim();
            return await _context.ProductOffers
                .AnyAsync(o =>
                    o.ProductId == productId &&
                    o.SupplierId == supplierId &&
                    o.VariantValue == value &&
                    !o.IsDeleted);
        }
        public async Task<bool> HasActiveOfferAsync(int productId, int supplierId)
        {
            return await _context.ProductOffers
                .AnyAsync(o =>
                    o.ProductId == productId &&
                    o.SupplierId == supplierId &&
                    o.IsActive &&
                    !o.IsDeleted);
        }
    }
      
}
