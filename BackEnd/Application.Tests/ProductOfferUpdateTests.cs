using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Application.Commands;
using Application.Common;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Xunit;

namespace Application.Tests;

public class ProductOfferUpdateTests
{
    [Fact]
    public void ContentEditor_principal_is_catalog_staff()
    {
        var http = new DefaultHttpContext
        {
            User = CatalogApiKey.CreatePrincipal(9, "content-bot@onlineshop.local", "Content Admin")
        };

        Assert.True(http.IsCatalogStaff());
    }

    [Fact]
    public void Customer_principal_is_not_catalog_staff()
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "4"),
                    new Claim(ClaimTypes.Role, "Customer")
                ],
                "Bearer"))
        };

        Assert.False(http.IsCatalogStaff());
    }

    [Fact]
    public async Task ContentEditor_can_update_offer_owned_by_another_supplier()
    {
        var offer = ProductOffers.Create(10, supplierId: 1, basePrice: 1_000_000, inventory: 4, currentUserId: 1);
        SetId(offer, 7);
        var store = new OfferStore { Offer = offer };
        var handler = CreateHandler(store, CatalogApiKey.CreatePrincipal(99, "bot@shop.local", "Bot"));

        var result = await handler.Handle(
            new UpdateProductOfferCommand { Id = 7, BasePrice = 1_250_000, Inventory = 2 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1_250_000, store.Offer.BasePrice);
        Assert.Equal(2, store.Offer.Inventory);
        Assert.Equal(7, result.Data!.Id);
    }

    [Fact]
    public async Task Non_staff_cannot_update_someone_elses_offer()
    {
        var offer = ProductOffers.Create(10, supplierId: 1, basePrice: 1_000_000, inventory: 4, currentUserId: 1);
        SetId(offer, 7);
        var store = new OfferStore { Offer = offer };
        var customer = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "99"),
                new Claim(ClaimTypes.Role, "Customer")
            ],
            "Bearer"));
        var handler = CreateHandler(store, customer);

        var result = await handler.Handle(
            new UpdateProductOfferCommand { Id = 7, BasePrice = 9 },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unauthorized", result.Error);
        Assert.Equal(1_000_000, store.Offer.BasePrice);
    }

    private static ProductOfferCommandHandler CreateHandler(OfferStore store, ClaimsPrincipal user)
    {
        return new ProductOfferCommandHandler(
            new FakeOfferRepository(store),
            new UnusedProducts(),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user } });
    }

    private static void SetId(BaseEntity entity, int id) =>
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(entity, id);

    private sealed class OfferStore
    {
        public ProductOffers? Offer { get; set; }
    }

    private sealed class FakeOfferRepository(OfferStore store) : IProductOfferRepository
    {
        public Task<ProductOffers?> GetByIdAsync(int id) =>
            Task.FromResult(store.Offer != null && store.Offer.Id == id ? store.Offer : null);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task AddAsync(ProductOffers entity) => throw new NotImplementedException();
        public Task<List<ProductOffers>> GetAllAsync(Expression<Func<ProductOffers, bool>>? predicate = null) => throw new NotImplementedException();
        public IQueryable<ProductOffers> Query(Expression<Func<ProductOffers, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(ProductOffers entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<ProductOffers?> GetByIdWithDetailsAsync(int offerId) => throw new NotImplementedException();
        public Task<ProductOffers?> GetByProductAndSupplierAsync(int productId, int supplierId) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(int productId, int supplierId) => throw new NotImplementedException();
        public Task<bool> HasActiveOfferAsync(int productId, int supplierId) => throw new NotImplementedException();
    }

    private sealed class UnusedProducts : IProductRepository
    {
        public Task AddAsync(Product entity) => throw new NotImplementedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsByNameAndBrandAsync(string name, int? brandId) => throw new NotImplementedException();
        public Task<bool> ExistsBySlugAsync(string slug, int? excludeProductId = null) => throw new NotImplementedException();
        public Task<Product?> GetByIdAsync(int id) => throw new NotImplementedException();
        public Task<List<Product>> GetAllAsync(Expression<Func<Product, bool>>? predicate = null) => throw new NotImplementedException();
        public IQueryable<Product> Query(Expression<Func<Product, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(Product entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<Product?> GetProductByIdAsync(int productId) => throw new NotImplementedException();
        public Task<Product?> GetProductWithDetailsAsync(int productId) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetAllProductsAsync() => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByCategoryIdAsync(int categoryId) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(int brandId) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> SearchByNameAsync(string keyword) => throw new NotImplementedException();
        public Task AddSpecificationAsync(int productId, string key, string value, int userId) => throw new NotImplementedException();
        public Task RemoveSpecificationAsync(int productId, string key, int userId) => throw new NotImplementedException();
        public Task SetDimensionsAsync(int productId, decimal width, decimal height, decimal depth, decimal weight, int userId) => throw new NotImplementedException();
    }
}
