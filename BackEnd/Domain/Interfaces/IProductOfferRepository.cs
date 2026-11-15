using OnlineShop.Domain.Interfaces;

public interface IProductOfferRepository : IRepository<ProductOffers>
{
    // ===== Read =====
    Task<ProductOffers?> GetByIdWithDetailsAsync(int offerId);
    Task<ProductOffers?> GetByProductAndSupplierAsync(
        int productId,
        int supplierId);

    // ===== Business =====
    Task<bool> ExistsAsync(int productId, int supplierId, string? variantValue = null);
    Task<bool> HasActiveOfferAsync(int productId, int supplierId);

}
