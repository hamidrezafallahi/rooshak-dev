using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Persistence;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductModel3DRepository : Repository<ProductModel3D>, IProductModel3DRepository
    {
        public ProductModel3DRepository(AppDbContext context) : base(context) { }

        public Task<ProductModel3D?> GetByProductIdAsync(int productId) =>
            Query(m => m.ProductId == productId).FirstOrDefaultAsync();
    }

    public class ProductScanSourceRepository : Repository<ProductScanSource>, IProductScanSourceRepository
    {
        public ProductScanSourceRepository(AppDbContext context) : base(context) { }

        public Task<List<ProductScanSource>> GetByProductIdAsync(int productId) =>
            Query(s => s.ProductId == productId).OrderBy(s => s.Id).ToListAsync();
    }
}
