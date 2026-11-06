using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Application.Commands;
using Application.Common.Interfaces;
using Application.Handler.CommandHandler;
using Common;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.ValueObjects;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using Xunit;

namespace Application.Tests;

/// <summary>
/// A file that cannot be stored must leave things as they were: no half-saved record after a
/// create, and the old file still in place after an edit. The uploader may answer that in two
/// ways, with null or with an exception, so every case runs with both.
/// </summary>
public class UploadFailureTests
{
    public enum Failure
    {
        ReturnsNull,
        Throws,
    }

    private const string Stored = "uploads/test/stored.webp";

    private static readonly IFormFile File =
        new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "picture.jpg");

    private static async Task<bool> FailsAsync(Func<Task<ServiceResult<IdDto>>> run)
    {
        try
        {
            return !(await run()).IsSuccess;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    // ── Create: no record stays behind, and the next try works ─────────────

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Brand_is_not_kept_when_its_logo_fails(Failure failure)
    {
        var table = new Table<Brand>();
        var command = new CreateBrandCommand { Name = "Vista", Description = "x", LogoFile = File };

        Assert.True(await FailsAsync(() => Brands(table, new FakeUploader(failure)).Handle(command, default)));
        Assert.Empty(table.Rows);

        var retry = await Brands(table, new FakeUploader()).Handle(command, default);

        Assert.True(retry.IsSuccess);
        Assert.Equal(Stored, Assert.Single(table.Rows).LogoUrl);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Category_is_not_kept_when_its_cover_fails(Failure failure)
    {
        var table = new Table<Category>();
        var command = new CreateCategoryCommand
        {
            PersianName = "لوپ",
            EnglishName = "Loupe",
            CategoryPersianDesc = "توضیح",
            CategoryEnglishDesc = "Text",
            CategoryCover = File,
        };

        Assert.True(await FailsAsync(() => Categories(table, new FakeUploader(failure)).Handle(command, default)));
        Assert.Empty(table.Rows);

        var retry = await Categories(table, new FakeUploader()).Handle(command, default);

        Assert.True(retry.IsSuccess);
        Assert.Equal(Stored, Assert.Single(table.Rows).ImageUrl);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Blog_is_not_kept_when_its_image_fails(Failure failure)
    {
        var table = new Table<Blog>();
        var command = new CreateBlogCommand
        {
            TitleFa = "عنوان",
            IntroFa = "مقدمه",
            ContentFa = "متن",
            ConclusionFa = "پایان",
            ThumbnailFile = File,
        };

        Assert.True(await FailsAsync(() => Blogs(table, new FakeUploader(failure)).Handle(command, default)));
        Assert.Empty(table.Rows);

        var retry = await Blogs(table, new FakeUploader()).Handle(command, default);

        Assert.True(retry.IsSuccess);
        Assert.Equal(Stored, Assert.Single(table.Rows).ThumbnailFile);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Slide_is_not_kept_when_its_banner_fails(Failure failure)
    {
        var table = new Table<Slide>();

        Assert.True(await FailsAsync(() => Slides(table, new FakeUploader(failure)).Handle(NewSlide(), default)));
        Assert.Empty(table.Rows);

        var retry = await Slides(table, new FakeUploader()).Handle(NewSlide(), default);

        Assert.True(retry.IsSuccess);
        Assert.Equal(Stored, Assert.Single(table.Rows).BannerUrl);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Slide_is_not_kept_when_a_later_file_fails(Failure failure)
    {
        var table = new Table<Slide>();
        var command = NewSlide();
        command.VideoUrl = File;
        var uploader = new FakeUploader(failure) { GoodUploads = 1 };

        Assert.True(await FailsAsync(() => Slides(table, uploader).Handle(command, default)));

        Assert.Empty(table.Rows);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Announcement_bar_is_not_kept_when_its_image_fails(Failure failure)
    {
        var table = new Table<AnnouncementBar>();
        var command = new CreateAnnouncementBarCommand { MessageFa = "اعلان", BackgroundImageUrl = File };

        Assert.True(await FailsAsync(() => Bars(table, new FakeUploader(failure)).Handle(command, default)));
        Assert.Empty(table.Rows);

        var retry = await Bars(table, new FakeUploader()).Handle(command, default);

        Assert.True(retry.IsSuccess);
        Assert.Equal(Stored, Assert.Single(table.Rows).BackgroundImageUrl);
    }

    [Fact]
    public async Task A_record_without_a_file_is_saved_as_before()
    {
        var table = new Table<Brand>();
        var uploader = new FakeUploader(Failure.Throws);

        var result = await Brands(table, uploader)
            .Handle(new CreateBrandCommand { Name = "Vista", Description = "x" }, default);

        Assert.True(result.IsSuccess);
        Assert.Single(table.Rows);
    }

    // ── Edit: the current file survives until the new one is saved ─────────

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_edit_keeps_the_brand_logo(Failure failure)
    {
        var table = new Table<Brand>();
        var created = await Brands(table, new FakeUploader())
            .Handle(new CreateBrandCommand { Name = "Vista", Description = "x", LogoFile = File }, default);
        var command = new UpdateBrandCommand { Id = created.Data.Id!.Value, LogoFile = File };
        var failing = new FakeUploader(failure);

        Assert.True(await FailsAsync(() => Brands(table, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, table.Rows[0].LogoUrl);

        var working = new FakeUploader { Path = "uploads/test/new.webp" };
        Assert.True((await Brands(table, working).Handle(command, default)).IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
        Assert.Equal("uploads/test/new.webp", table.Rows[0].LogoUrl);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_edit_keeps_the_category_cover(Failure failure)
    {
        var table = new Table<Category>();
        var created = await Categories(table, new FakeUploader()).Handle(
            new CreateCategoryCommand
            {
                PersianName = "لوپ",
                EnglishName = "Loupe",
                CategoryPersianDesc = "توضیح",
                CategoryEnglishDesc = "Text",
                CategoryCover = File,
            },
            default);
        var command = new UpdateCategoryCommand { Id = created.Data.Id!.Value, CategoryCover = File };
        var failing = new FakeUploader(failure);

        Assert.True(await FailsAsync(() => Categories(table, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, table.Rows[0].ImageUrl);

        var working = new FakeUploader { Path = "uploads/test/new.webp" };
        Assert.True((await Categories(table, working).Handle(command, default)).IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_edit_keeps_the_blog_image(Failure failure)
    {
        var table = new Table<Blog>();
        var created = await Blogs(table, new FakeUploader()).Handle(
            new CreateBlogCommand
            {
                TitleFa = "عنوان",
                IntroFa = "مقدمه",
                ContentFa = "متن",
                ConclusionFa = "پایان",
                ThumbnailFile = File,
            },
            default);
        var command = new UpdateBlogCommand { Id = created.Data.Id!.Value, ThumbnailFile = File };
        var failing = new FakeUploader(failure);

        Assert.True(await FailsAsync(() => Blogs(table, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, table.Rows[0].ThumbnailFile);

        var working = new FakeUploader { Path = "uploads/test/new.webp" };
        Assert.True((await Blogs(table, working).Handle(command, default)).IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_edit_keeps_every_slide_file(Failure failure)
    {
        var table = new Table<Slide>();
        var create = NewSlide();
        create.VideoUrl = File;
        var created = await Slides(table, new FakeUploader()).Handle(create, default);

        // The new banner is stored, then the video fails: nothing may be deleted yet.
        var command = new UpdateSlideCommand { Id = created.Data.Id!.Value, BannerUrl = File, VideoUrl = File };
        var failing = new FakeUploader(failure) { GoodUploads = 1, Path = "uploads/test/new.webp" };

        Assert.True(await FailsAsync(() => Slides(table, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, table.Rows[0].BannerUrl);
        Assert.Equal(Stored, table.Rows[0].VideoUrl);
    }

    [Fact]
    public async Task Removing_a_slide_video_deletes_the_file_after_the_save()
    {
        var table = new Table<Slide>();
        var create = NewSlide();
        create.VideoUrl = File;
        var firstUploader = new FakeUploader();
        var created = await Slides(table, firstUploader).Handle(create, default);
        var working = new FakeUploader { Path = "uploads/test/new.webp" };

        var result = await Slides(table, working).Handle(
            new UpdateSlideCommand { Id = created.Data.Id!.Value, RemoveVideo = true, BannerUrl = File },
            default);

        Assert.True(result.IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
        Assert.Equal(string.Empty, table.Rows[0].VideoUrl);
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_edit_keeps_the_announcement_image(Failure failure)
    {
        var table = new Table<AnnouncementBar>();
        var created = await Bars(table, new FakeUploader()).Handle(
            new CreateAnnouncementBarCommand { MessageFa = "اعلان", BackgroundImageUrl = File },
            default);
        var command = new UpdateAnnouncementBarCommand
        {
            Id = created.Data.Id!.Value,
            MessageFa = "اعلان",
            BackgroundImageUrl = File,
        };
        var failing = new FakeUploader(failure);

        Assert.True(await FailsAsync(() => Bars(table, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, table.Rows[0].BackgroundImageUrl);

        var working = new FakeUploader { Path = "uploads/test/new.webp" };
        Assert.True((await Bars(table, working).Handle(command, default)).IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_upload_keeps_the_current_product_images(Failure failure)
    {
        var products = new Table<Product>();
        var images = new Table<ProductImage>();
        var product = Product.Create("Loupe", "Text", 1, 1, new ProductDimensions(1, 1, 1, 1), 1);
        products.Rows.Add(product);
        images.Rows.Add(ProductImage.Create(product.Id, Stored, true, 1));
        var failing = new FakeUploader(failure) { GoodUploads = 1, Path = "uploads/test/new.webp" };
        var command = new AddProductImageCommand { ProductId = product.Id, Images = [File, File] };

        Assert.True(await FailsAsync(() => ProductImages(images, products, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.False(images.Rows[0].IsDeleted);
    }

    [Fact]
    public async Task New_product_images_replace_the_old_ones_after_the_save()
    {
        var products = new Table<Product>();
        var images = new Table<ProductImage>();
        var product = Product.Create("Loupe", "Text", 1, 1, new ProductDimensions(1, 1, 1, 1), 1);
        products.Rows.Add(product);
        images.Rows.Add(ProductImage.Create(product.Id, Stored, true, 1));
        var working = new FakeUploader { Path = "uploads/test/new.webp" };

        var result = await ProductImages(images, products, working).Handle(
            new AddProductImageCommand { ProductId = product.Id, Images = [File] },
            default);

        Assert.True(result.IsSuccess);
        Assert.True(images.Rows[0].IsDeleted);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
    }

    [Theory]
    [InlineData(Failure.ReturnsNull)]
    [InlineData(Failure.Throws)]
    public async Task Failed_upload_keeps_the_profile_picture(Failure failure)
    {
        var users = new Table<User>();
        var user = User.Create("Sara", "sara@example.com", "09120000000", "");
        user.SetImage(Stored, 1);
        users.Rows.Add(user);
        var failing = new FakeUploader(failure);
        var command = new UpdateUserProfileCommand { UserId = user.Id, FullName = "Sara", UserImageFile = File };

        Assert.True(await FailsAsync(() => Profiles(users, failing).Handle(command, default)));

        Assert.Empty(failing.Deleted);
        Assert.Equal(Stored, user.Image);

        var working = new FakeUploader { Path = "uploads/test/new.webp" };
        Assert.True((await Profiles(users, working).Handle(command, default)).IsSuccess);
        Assert.Equal(Stored, Assert.Single(working.Deleted));
    }

    // ── Wiring ─────────────────────────────────────────────────────────────

    private static CreateSlideCommand NewSlide() => new()
    {
        BannerUrl = File,
        BannerTitle = "عنوان",
        BannerDescription = "توضیح",
        FirstUrl = "/fa/products",
    };

    private static HttpContextAccessor SignedIn(int userId = 7) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test")),
        },
    };

    private static BrandCommandHandler Brands(Table<Brand> table, FakeUploader uploader) =>
        new(table.Repository<IBrandRepository>(), SignedIn(), uploader, table);

    private static CategoryCommandHandler Categories(Table<Category> table, FakeUploader uploader) =>
        new(table.Repository<ICategoryRepository>(), SignedIn(), uploader, table);

    private static BlogCommandHandler Blogs(Table<Blog> table, FakeUploader uploader) =>
        new(table.Repository<IBlogRepository>(), null!, SignedIn(), uploader, table);

    private static SlideCommandHandler Slides(Table<Slide> table, FakeUploader uploader) =>
        new(table.Repository<ISlideRepository>(), SignedIn(), uploader, table);

    private static AnnouncementBarCommandHandler Bars(Table<AnnouncementBar> table, FakeUploader uploader) =>
        new(table.Repository<IAnnouncementBarRepository>(), SignedIn(), uploader, table);

    private static AddProductImageCommandHandler ProductImages(
        Table<ProductImage> images,
        Table<Product> products,
        FakeUploader uploader) =>
        new(images.Repository<IProductImageRepository>(), products.Repository<IProductRepository>(), SignedIn(), uploader);

    private static UserCommandsHandler Profiles(Table<User> users, FakeUploader uploader) =>
        new(users.Repository<IUserRepository>(), null!, uploader, null!, SignedIn(users.Rows[0].Id));

    /// <summary>
    /// A table with the transaction semantics that matter here: rows added since the transaction
    /// began disappear unless it was committed. Rows get their id on save, as in the database.
    /// </summary>
    public sealed class Table<T> : IUnitOfWork where T : BaseEntity
    {
        private static readonly PropertyInfo Id = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!;

        private int _lastId;

        public List<T> Rows { get; } = [];

        public TRepository Repository<TRepository>() where TRepository : class
        {
            var proxy = DispatchProxy.Create<TRepository, RepositoryProxy<T>>();
            ((RepositoryProxy<T>)(object)proxy).Table = this;
            return proxy;
        }

        public void Save()
        {
            foreach (var row in Rows.Where(r => r.Id == 0))
                Id.SetValue(row, ++_lastId);
        }

        public Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IAppTransaction>(new Transaction(this));

        private sealed class Transaction(Table<T> table) : IAppTransaction
        {
            private readonly List<T> _before = [.. table.Rows];
            private bool _committed;

            public Task CommitAsync(CancellationToken cancellationToken = default)
            {
                _committed = true;
                return Task.CompletedTask;
            }

            public Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                if (!_committed)
                {
                    table.Rows.Clear();
                    table.Rows.AddRange(_before);
                }
                return Task.CompletedTask;
            }

            public async ValueTask DisposeAsync() => await RollbackAsync();
        }
    }

    /// <summary>Stands in for any repository interface; the handlers here only use these members.</summary>
    public class RepositoryProxy<T> : DispatchProxy where T : BaseEntity
    {
        public Table<T> Table { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Name switch
        {
            "AddAsync" => Add((T)args![0]!),
            "SaveChangesAsync" => Save(),
            "GetByIdAsync" => Task.FromResult(Table.Rows.FirstOrDefault(r => r.Id == (int)args![0]!)),
            "Query" => Filter((Expression<Func<T, bool>>?)args![0]),
            _ => throw new NotSupportedException(targetMethod.Name),
        };

        private Task Add(T row)
        {
            Table.Rows.Add(row);
            return Task.CompletedTask;
        }

        private Task<int> Save()
        {
            Table.Save();
            return Task.FromResult(1);
        }

        private IQueryable<T> Filter(Expression<Func<T, bool>>? predicate)
        {
            var rows = Table.Rows.AsQueryable();
            return predicate is null ? rows : rows.Where(predicate);
        }
    }

    public sealed class FakeUploader(Failure? failure = null) : IUploaderService
    {
        public string Path { get; init; } = Stored;

        /// <summary>How many uploads succeed before the failure starts.</summary>
        public int GoodUploads { get; init; }

        public List<string?> Deleted { get; } = [];

        private int _uploads;

        private Task<string?> Upload()
        {
            _uploads++;
            if (failure is null || _uploads <= GoodUploads)
                return Task.FromResult<string?>(Path);

            return failure == Failure.Throws
                ? throw new InvalidOperationException("The file could not be stored.")
                : Task.FromResult<string?>(null);
        }

        public Task<string?> UploadAsWebp(UploadDTO request) => Upload();
        public Task<string?> UploadAsPng(UploadDTO request) => Upload();
        public Task<string?> UploadAsJpeg(UploadDTO request) => Upload();
        public Task<string?> UploadAsJpg(UploadDTO request) => Upload();
        public Task<string?> UploadVideo(UploadDTO request) => Upload();
        public Task DeleteFile(DeleteDTO request) => Task.CompletedTask;

        public Task DeleteStoredFile(string? storedPath, string fallbackDirectory)
        {
            Deleted.Add(storedPath);
            return Task.CompletedTask;
        }
    }
}
