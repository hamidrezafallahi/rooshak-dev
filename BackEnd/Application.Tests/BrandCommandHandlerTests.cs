using System.Linq.Expressions;
using System.Security.Claims;
using Application.Commands;
using Application.Common.Interfaces;
using Application.Handler.CommandHandler;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using Xunit;

namespace Application.Tests;

public class BrandCommandHandlerTests
{
    [Fact]
    public async Task Create_with_logo_keeps_brand_active()
    {
        var repo = new FakeBrandRepository();
        var handler = CreateHandler(repo, userId: 3);

        var result = await handler.Handle(
            new CreateBrandCommand
            {
                Name = "Sample Brand",
                Description = "Sample description",
                LogoFile = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "LogoFile", "logo.png"),
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var brand = Assert.Single(repo.Brands);
        Assert.Equal("uploads/brands/1/logo.webp", brand.LogoUrl);
        Assert.True(brand.IsActive);
    }

    private static BrandCommandHandler CreateHandler(FakeBrandRepository repo, int userId)
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "test"))
        };

        return new BrandCommandHandler(
            repo,
            new HttpContextAccessor { HttpContext = http },
            new FakeUploader(),
            new FakeUnitOfWork());
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        public List<Brand> Brands { get; } = [];

        public Task AddAsync(Brand entity)
        {
            Brands.Add(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task<Brand?> GetByIdAsync(int id) => Task.FromResult(Brands.FirstOrDefault(b => b.Id == id));
        public Task<List<Brand>> GetAllAsync(Expression<Func<Brand, bool>>? predicate = null) => throw new NotSupportedException();
        public IQueryable<Brand> Query(Expression<Func<Brand, bool>>? predicate = null) => throw new NotSupportedException();
        public Task UpdateAsync(Brand entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<List<int>> GetAllIds() => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IAppTransaction>(new FakeTransaction());

        private sealed class FakeTransaction : IAppTransaction
        {
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    private sealed class FakeUploader : IUploaderService
    {
        public Task<string?> UploadAsWebp(UploadDTO request) =>
            Task.FromResult<string?>("uploads/brands/1/logo.webp");

        public Task<string?> UploadAsPng(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpeg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadVideo(UploadDTO request) => UploadAsWebp(request);
        public Task DeleteFile(DeleteDTO request) => Task.CompletedTask;
        public Task DeleteStoredFile(string? storedPath, string fallbackDirectory) => Task.CompletedTask;
    }
}
