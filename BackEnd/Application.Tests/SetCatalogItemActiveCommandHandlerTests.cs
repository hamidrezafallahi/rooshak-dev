using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Application.Commands;
using Application.Common.Interfaces;
using Application.Handler.CommandHandler;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.ValueObjects;
using Xunit;

namespace Application.Tests;

public class SetCatalogItemActiveCommandHandlerTests
{
    [Fact]
    public async Task Publish_activates_product_offer_and_image()
    {
        var store = SeedDraft();
        var handler = CreateHandler(store, userId: 9);

        var result = await handler.Handle(
            new SetCatalogItemActiveCommand { ProductId = 7, IsActive = true },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(store.Products[0].IsActive);
        Assert.True(store.Offers[0].IsActive);
        Assert.True(store.Images[0].IsActive);
        Assert.Equal(7, result.Data!.ProductId);
        Assert.True(result.Data.IsActive);
        Assert.Equal(1, result.Data.OfferCount);
        Assert.Equal(1, result.Data.ImageCount);
    }

    [Fact]
    public async Task Unpublish_deactivates_product_offer_and_image()
    {
        var store = SeedDraft();
        store.Products[0].SetActive(true, 9);
        store.Offers[0].SetActive(true, 9);
        store.Images[0].SetActive(true, 9);
        var handler = CreateHandler(store, userId: 9);

        var result = await handler.Handle(
            new SetCatalogItemActiveCommand { Id = 7, IsActive = false },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(store.Products[0].IsActive);
        Assert.False(store.Offers[0].IsActive);
        Assert.False(store.Images[0].IsActive);
        Assert.False(result.Data!.IsActive);
    }

    [Fact]
    public async Task Publish_without_offer_does_not_activate_product()
    {
        var store = SeedDraft();
        store.Offers.Clear();
        var handler = CreateHandler(store, userId: 9);

        var result = await handler.Handle(
            new SetCatalogItemActiveCommand { Id = 7, IsActive = true },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product has no offer to publish", result.Error);
        Assert.False(store.Products[0].IsActive);
        Assert.False(store.Images[0].IsActive);
    }

    [Fact]
    public async Task Missing_user_is_unauthorized()
    {
        var store = SeedDraft();
        var handler = CreateHandler(store, userId: null);

        var result = await handler.Handle(
            new SetCatalogItemActiveCommand { Id = 7, IsActive = true },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Unauthorized", result.Error);
        Assert.False(store.Products[0].IsActive);
    }

    [Fact]
    public async Task Missing_product_returns_not_found()
    {
        var store = SeedDraft();
        var handler = CreateHandler(store, userId: 9);

        var result = await handler.Handle(
            new SetCatalogItemActiveCommand { Id = 99, IsActive = true },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
        Assert.False(store.Products[0].IsActive);
        Assert.False(store.Offers[0].IsActive);
    }

    private static CatalogStore SeedDraft()
    {
        var store = new CatalogStore();
        var product = Product.Create(
            "P017 draft",
            "test",
            categoryId: 3,
            brandId: 2,
            new ProductDimensions(0, 0, 0, 0),
            currentUserId: 9);
        SetId(product, 7);
        product.SetActive(false, 9);

        var offer = ProductOffers.Create(7, supplierId: 9, basePrice: 10000, inventory: 1, currentUserId: 9);
        SetId(offer, 7);
        offer.SetActive(false, 9);

        var image = ProductImage.Create(7, "uploads/products/7/main.webp", isMain: true, currentUserId: 9);
        SetId(image, 15);
        image.SetActive(false, 9);

        store.Products.Add(product);
        store.Offers.Add(offer);
        store.Images.Add(image);
        return store;
    }

    private static SetCatalogItemActiveCommandHandler CreateHandler(CatalogStore store, int? userId)
    {
        var http = new DefaultHttpContext();
        if (userId.HasValue)
        {
            http.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())],
                "test"));
        }

        return new SetCatalogItemActiveCommandHandler(
            new FakeProducts(store),
            new FakeOffers(store),
            new FakeImages(store),
            new FakeUnitOfWork(),
            new HttpContextAccessor { HttpContext = http });
    }

    private static void SetId(BaseEntity entity, int id) =>
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(entity, id);

    private sealed class CatalogStore
    {
        public List<Product> Products { get; } = [];
        public List<ProductOffers> Offers { get; } = [];
        public List<ProductImage> Images { get; } = [];
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IAppTransaction>(new FakeTransaction());
    }

    private sealed class FakeTransaction : IAppTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeProducts(CatalogStore store) : IProductRepository
    {
        public Task<Product?> GetByIdAsync(int id) =>
            Task.FromResult(store.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task AddAsync(Product entity) => throw new NotImplementedException();
        public Task<List<Product>> GetAllAsync(Expression<Func<Product, bool>>? predicate = null) => throw new NotImplementedException();
        public IQueryable<Product> Query(Expression<Func<Product, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(Product entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<bool> ExistsByNameAndBrandAsync(string name, int? brandId) => throw new NotImplementedException();
        public Task<bool> ExistsBySlugAsync(string slug, int? excludeProductId = null) => throw new NotImplementedException();
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

    private sealed class FakeOffers(CatalogStore store) : IProductOfferRepository
    {
        public Task<List<ProductOffers>> GetAllAsync(Expression<Func<ProductOffers, bool>>? predicate = null)
        {
            IEnumerable<ProductOffers> query = store.Offers.Where(o => !o.IsDeleted);
            if (predicate != null)
                query = query.Where(predicate.Compile());
            return Task.FromResult(query.ToList());
        }

        public Task AddAsync(ProductOffers entity) => throw new NotImplementedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProductOffers?> GetByIdAsync(int id) => throw new NotImplementedException();
        public IQueryable<ProductOffers> Query(Expression<Func<ProductOffers, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(ProductOffers entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<ProductOffers?> GetByIdWithDetailsAsync(int offerId) => throw new NotImplementedException();
        public Task<ProductOffers?> GetByProductAndSupplierAsync(int productId, int supplierId) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(int productId, int supplierId) => throw new NotImplementedException();
        public Task<bool> HasActiveOfferAsync(int productId, int supplierId) => throw new NotImplementedException();
    }

    private sealed class FakeImages(CatalogStore store) : IProductImageRepository
    {
        public Task<List<ProductImage>> GetAllAsync(Expression<Func<ProductImage, bool>>? predicate = null)
        {
            IEnumerable<ProductImage> query = store.Images.Where(i => !i.IsDeleted);
            if (predicate != null)
                query = query.Where(predicate.Compile());
            return Task.FromResult(query.ToList());
        }

        public Task AddAsync(ProductImage entity) => throw new NotImplementedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ProductImage?> GetByIdAsync(int id) => throw new NotImplementedException();
        public IQueryable<ProductImage> Query(Expression<Func<ProductImage, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(ProductImage entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId) => throw new NotImplementedException();
        public Task<ProductImage?> GetMainImageByProductIdAsync(int productId) => throw new NotImplementedException();
        public Task<bool> DeleteImagesByProductIdAsync(int productId) => throw new NotImplementedException();
    }
}
