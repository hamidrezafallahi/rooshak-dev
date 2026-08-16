using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.ValueObjects;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // ===================== Read =====================

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await QueryWithAggregate().ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await QueryWithAggregate()
                         .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await QueryWithAggregate()
                         .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                         .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByBrandIdAsync(int brandId)
        {
            return await QueryWithAggregate()
                         .Where(p => p.BrandId == brandId && !p.IsDeleted)
                         .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetDiscountedProductsAsync()
        {
            var now = DateTime.UtcNow;
            return await QueryWithAggregate()
                 .Where(p => p.ProductOffers
                     .Any(po => !po.IsDeleted &&
                                po.Discounts
                                  .Any(pd => !pd.IsDeleted &&
                                             pd.Discount.StartDate <= now &&
                                             pd.Discount.EndDate >= now)))
                 .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Product>();

            keyword = keyword.ToLower();
            return await QueryWithAggregate()
                         .Where(p => !p.IsDeleted && p.IsActive && p.Name.ToLower().Contains(keyword))
                         .ToListAsync();
        }

        // ===================== Aggregate Helpers =====================


        private IQueryable<Product> QueryWithAggregate()
        {
            return Query()
                   .Include(p => p.Category)
                   .Include(p => p.Brand)
                   .Include(p => p.Images)
                   .Include(p => p.ProductOffers)  
                       .ThenInclude(po => po.Discounts)  
                           .ThenInclude(pd => pd.Discount)
                   .Include(p => p.ProductOffers).ThenInclude(po=>po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                   .Include(p => p.Specifications)




                   ;
        }
        // ===================== Specifications =====================

        public async Task AddSpecificationAsync(int productId, string key, string value, int userId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            product.AddSpecification(key, value);
            await UpdateAsync(product);
        }

        public async Task RemoveSpecificationAsync(int productId, string key, int userId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            product.RemoveSpecification(key, userId);
            await UpdateAsync(product);
        }

        // ===================== Dimensions =====================

        public async Task SetDimensionsAsync(int productId,
                                             decimal width,
                                             decimal height,
                                             decimal depth,
                                             decimal weight,
                                             int userId)
        {
            var product = await GetProductByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            product.SetDimensions(new ProductDimensions(width, height, depth, weight));
            await UpdateAsync(product);
        }

        public async Task<Product?> GetProductWithDetailsAsync(int productId)
        {
            return await QueryWithAggregate()
                         .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);
        }

        public async Task<bool> ExistsByNameAndBrandAsync(string name, int? brandId)
        {
            return await _context.Products
                .AnyAsync(p => !p.IsDeleted &&
                               p.Name.Trim().ToLower() == name.Trim().ToLower() &&
                               p.BrandId == brandId);
        }

        public async Task<bool> ExistsBySlugAsync(string slug, int? excludeProductId = null)
        {
            var normalized = slug.Trim().ToLower();
            return await _context.Products.AnyAsync(p =>
                !p.IsDeleted
                && p.Slug.ToLower() == normalized
                && (!excludeProductId.HasValue || p.Id != excludeProductId.Value));
        }
    }
}
