using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces
{
    public interface IProductModel3DRepository : IRepository<ProductModel3D>
    {
        Task<ProductModel3D?> GetByProductIdAsync(int productId);
    }

    public interface IProductScanSourceRepository : IRepository<ProductScanSource>
    {
        Task<List<ProductScanSource>> GetByProductIdAsync(int productId);
    }
}
