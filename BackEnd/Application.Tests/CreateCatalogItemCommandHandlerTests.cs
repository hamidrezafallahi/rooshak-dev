using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Handler.CommandHandler;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using Xunit;

namespace Application.Tests;

public class CreateCatalogItemCommandHandlerTests
{
    [Fact]
    public async Task Failed_offer_does_not_leave_orphan_product()
    {
        var store = new FakeCatalogStore { ThrowOnOfferAdd = true };
        var handler = CreateHandler(store, userId: 7);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Catalog item could not be created as a complete Product+Offer unit.", result.Error);
        Assert.Empty(store.Products);
        Assert.Empty(store.Offers);
        Assert.Empty(store.Images);
    }

    [Fact]
    public async Task Complete_item_persists_product_offer_and_optional_image()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7);

        var command = ValidCommand();
        command.ImageUrl = "/uploads/products/1/main.webp";

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(store.Products);
        Assert.Single(store.Offers);
        Assert.Single(store.Images);
        Assert.Equal(store.Products[0].Id, result.Data.ProductId);
        Assert.Equal(store.Offers[0].Id, result.Data.OfferId);
        Assert.Equal(store.Images[0].Id, result.Data.ImageId);
        Assert.Equal(store.Products[0].Id, store.Offers[0].ProductId);
        Assert.Equal(7, store.Offers[0].SupplierId);
        Assert.False(store.Products[0].IsActive);
        Assert.False(store.Offers[0].IsActive);
        Assert.False(store.Images[0].IsActive);
    }

    [Fact]
    public async Task Catalog_item_is_always_created_inactive()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(store.Products[0].IsActive);
        Assert.False(store.Offers[0].IsActive);
    }

    [Fact]
    public async Task Invalid_offer_price_does_not_start_transaction()
    {
        var store = new FakeCatalogStore();
        var unitOfWork = new FakeUnitOfWork(store);
        var handler = CreateHandler(store, userId: 7, unitOfWork);

        var command = ValidCommand();
        command.BasePrice = 0;

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Offer base price must be greater than zero.", result.Error);
        Assert.Equal(0, unitOfWork.BeginCount);
        Assert.Empty(store.Products);
    }

    [Fact]
    public async Task Failed_image_upload_does_not_leave_orphan_product()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7, uploader: new FakeUploader { Fail = true });
        var command = ValidCommand();
        command.ImageFile = TinyJpeg();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Catalog item could not be created as a complete Product+Offer unit.", result.Error);
        Assert.Empty(store.Products);
        Assert.Empty(store.Offers);
        Assert.Empty(store.Images);
    }

    [Fact]
    public async Task Uploaded_image_file_is_stored_on_the_draft()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7);
        var command = ValidCommand();
        command.ImageFile = TinyJpeg();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(store.Images);
        Assert.Equal("uploads/products/1/test.webp", store.Images[0].ImageUrl);
        Assert.False(store.Images[0].IsActive);
        Assert.Equal(store.Images[0].Id, result.Data!.ImageId);
    }

    [Fact]
    public async Task Vessel_diameter_and_height_are_stored_as_specifications()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7);

        var command = ValidCommand();
        command.Name = "گلدان کریستال P019";
        command.Diameter = 18;
        command.Height = 32;
        command.PieceCount = 1;

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var specs = store.Products[0].Specifications.ToList();
        Assert.Equal(3, specs.Count);
        Assert.Contains(specs, s => s.Key == VesselCatalogSpecs.DiameterKey && s.Value.Contains("18"));
        Assert.Contains(specs, s => s.Key == VesselCatalogSpecs.HeightKey && s.Value.Contains("32"));
        Assert.Contains(specs, s => s.Key == VesselCatalogSpecs.PieceCountKey && s.Value == "1");
        Assert.DoesNotContain(specs, s => s.Key.Contains("ml", StringComparison.OrdinalIgnoreCase)
            || s.Key.Contains("عطر", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Create_without_vessel_fields_adds_no_specifications()
    {
        var store = new FakeCatalogStore();
        var handler = CreateHandler(store, userId: 7);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(store.Products[0].Specifications);
    }

    private static CreateCatalogItemCommand ValidCommand() => new()
    {
        Name = "گلدان بلور",
        Description = "گلدان تست",
        CategoryId = 3,
        BrandId = 2,
        BasePrice = 1_500_000,
        Inventory = 4
    };

    private static IFormFile TinyJpeg()
    {
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "ImageFile", "gate-b.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private static CreateCatalogItemCommandHandler CreateHandler(
        FakeCatalogStore store,
        int userId,
        FakeUnitOfWork? unitOfWork = null,
        FakeUploader? uploader = null)
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "test"))
        };

        return new CreateCatalogItemCommandHandler(
            new FakeProductRepository(store),
            new FakeProductOfferRepository(store),
            new FakeProductImageRepository(store),
            unitOfWork ?? new FakeUnitOfWork(store),
            new HttpContextAccessor { HttpContext = http },
            uploader ?? new FakeUploader());
    }

    private sealed class FakeCatalogStore
    {
        public List<Product> Products { get; } = [];
        public List<ProductOffers> Offers { get; } = [];
        public List<ProductImage> Images { get; } = [];
        public int NextId { get; set; } = 1;
        public bool ThrowOnOfferAdd { get; set; }

        public int AssignId(BaseEntity entity)
        {
            if (entity.Id != 0)
                return entity.Id;

            var id = NextId++;
            typeof(BaseEntity)
                .GetProperty(nameof(BaseEntity.Id), BindingFlags.Instance | BindingFlags.Public)!
                .SetValue(entity, id);
            return id;
        }
    }

    private sealed class FakeUnitOfWork(FakeCatalogStore store) : IUnitOfWork
    {
        public int BeginCount { get; private set; }

        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            BeginCount++;
            return Task.FromResult<IAppTransaction>(new FakeTransaction(store));
        }
    }

    private sealed class FakeTransaction : IAppTransaction
    {
        private readonly FakeCatalogStore _store;
        private readonly List<Product> _products;
        private readonly List<ProductOffers> _offers;
        private readonly List<ProductImage> _images;
        private readonly int _nextId;
        private bool _completed;

        public FakeTransaction(FakeCatalogStore store)
        {
            _store = store;
            _products = [.. store.Products];
            _offers = [.. store.Offers];
            _images = [.. store.Images];
            _nextId = store.NextId;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            _completed = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_completed)
                return Task.CompletedTask;

            Restore();
            _completed = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            if (!_completed)
                Restore();
            return ValueTask.CompletedTask;
        }

        private void Restore()
        {
            _store.Products.Clear();
            _store.Products.AddRange(_products);
            _store.Offers.Clear();
            _store.Offers.AddRange(_offers);
            _store.Images.Clear();
            _store.Images.AddRange(_images);
            _store.NextId = _nextId;
        }
    }

    private sealed class FakeProductRepository(FakeCatalogStore store) : IProductRepository
    {
        public Task AddAsync(Product entity)
        {
            store.Products.Add(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var product in store.Products)
                store.AssignId(product);
            return Task.FromResult(store.Products.Count);
        }

        public Task<bool> ExistsByNameAndBrandAsync(string name, int? brandId)
        {
            var exists = store.Products.Any(p =>
                !p.IsDeleted
                && p.Name.Trim().Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)
                && p.BrandId == brandId);
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsBySlugAsync(string slug, int? excludeProductId = null)
        {
            var normalized = slug.Trim().ToLowerInvariant();
            var exists = store.Products.Any(p =>
                !p.IsDeleted
                && p.Slug.ToLowerInvariant() == normalized
                && (!excludeProductId.HasValue || p.Id != excludeProductId.Value));
            return Task.FromResult(exists);
        }

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

    private sealed class FakeProductOfferRepository(FakeCatalogStore store) : IProductOfferRepository
    {
        public Task AddAsync(ProductOffers entity)
        {
            if (store.ThrowOnOfferAdd)
                throw new InvalidOperationException("Offer persist failed.");

            store.Offers.Add(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var offer in store.Offers)
                store.AssignId(offer);
            foreach (var image in store.Images)
                store.AssignId(image);
            return Task.FromResult(store.Offers.Count);
        }

        public Task<ProductOffers?> GetByIdAsync(int id) => throw new NotImplementedException();
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

    private sealed class FakeProductImageRepository(FakeCatalogStore store) : IProductImageRepository
    {
        public Task AddAsync(ProductImage entity)
        {
            store.Images.Add(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var image in store.Images)
                store.AssignId(image);
            return Task.FromResult(store.Images.Count);
        }

        public Task<ProductImage?> GetByIdAsync(int id) => throw new NotImplementedException();
        public Task<List<ProductImage>> GetAllAsync(Expression<Func<ProductImage, bool>>? predicate = null) => throw new NotImplementedException();
        public IQueryable<ProductImage> Query(Expression<Func<ProductImage, bool>>? predicate = null) => throw new NotImplementedException();
        public Task UpdateAsync(ProductImage entity) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
        public Task<List<int>> GetAllIds() => throw new NotImplementedException();
        public Task<IEnumerable<ProductImage>> GetImagesByProductIdAsync(int productId) => throw new NotImplementedException();
        public Task<ProductImage?> GetMainImageByProductIdAsync(int productId) => throw new NotImplementedException();
        public Task<bool> DeleteImagesByProductIdAsync(int productId) => throw new NotImplementedException();
    }

    private sealed class FakeUploader : IUploaderService
    {
        public bool Fail { get; set; }

        public Task<string?> UploadAsWebp(UploadDTO request) =>
            Task.FromResult<string?>(Fail ? "upload failed" : "uploads/products/1/test.webp");

        public Task<string?> UploadAsPng(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpeg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadVideo(UploadDTO request) => UploadAsWebp(request);
        public Task DeleteFile(DeleteDTO request) => Task.CompletedTask;
        public Task DeleteStoredFile(string? storedPath, string fallbackDirectory) => Task.CompletedTask;
    }
}
