using System.Linq.Expressions;
using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Handler.CommandHandler;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using Xunit;

namespace Application.Tests;

public class UpdateUserProfileCommandHandlerTests
{
    private const string OldImage = "uploads/users/0/old.webp";

    [Fact]
    public async Task Edit_without_image_saves_the_new_fields()
    {
        var repo = new FakeUserRepository(ExistingUser());
        var uploader = new FakeUploader();

        var result = await CreateHandler(repo, uploader).Handle(Edit(repo.User), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new SavedProfile("New Name", "09121111111", "New description", OldImage),
            repo.LastSaved);
        Assert.Empty(uploader.Deleted);
        Assert.Empty(uploader.Uploaded);
    }

    [Fact]
    public async Task Edit_with_image_saves_the_new_fields_and_replaces_the_image()
    {
        var repo = new FakeUserRepository(ExistingUser());
        var uploader = new FakeUploader();

        var result = await CreateHandler(repo, uploader).Handle(Edit(repo.User, Image()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new SavedProfile("New Name", "09121111111", "New description", FakeUploader.StoredPath),
            repo.LastSaved);
        Assert.Equal([OldImage], uploader.Deleted);
        Assert.Equal(["uploads/users/0"], uploader.Uploaded);
    }

    [Fact]
    public async Task Edit_with_a_failed_upload_saves_nothing()
    {
        var repo = new FakeUserRepository(ExistingUser());
        var uploader = new FakeUploader { UploadFails = true };

        var result = await CreateHandler(repo, uploader).Handle(Edit(repo.User, Image()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(repo.LastSaved);
        Assert.Empty(uploader.Deleted);
    }

    [Fact]
    public async Task Edit_keeps_the_fields_it_does_not_send()
    {
        var repo = new FakeUserRepository(ExistingUser());

        var result = await CreateHandler(repo, new FakeUploader()).Handle(
            new UpdateUserProfileCommand { UserId = repo.User.Id, FullName = "" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new SavedProfile("Old Name", "09120000000", "Old description", OldImage),
            repo.LastSaved);
    }

    private static UpdateUserProfileCommand Edit(User user, IFormFile? image = null) => new()
    {
        UserId = user.Id,
        FullName = "New Name",
        PhoneNumber = "09121111111",
        UserDescription = "New description",
        UserImageFile = image,
    };

    private static FormFile Image() =>
        new(new MemoryStream([1, 2, 3]), 0, 3, "UserImageFile", "avatar.jpg");

    private static User ExistingUser()
    {
        var user = User.Create("Old Name", "user@example.com", "09120000000", "Old description");
        user.SetImage(OldImage, user.Id);
        return user;
    }

    private static UserCommandsHandler CreateHandler(FakeUserRepository repo, FakeUploader uploader)
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "3")],
                "test"))
        };

        return new UserCommandsHandler(
            repo,
            null!,
            uploader,
            null!,
            new HttpContextAccessor { HttpContext = http });
    }

    private sealed record SavedProfile(
        string FullName,
        string PhoneNumber,
        string? UserDescription,
        string Image);

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public User User { get; } = user;

        /// <summary>The profile as it stood at the last SaveChangesAsync call.</summary>
        public SavedProfile? LastSaved { get; private set; }

        public Task<User?> GetByIdAsync(int id) => Task.FromResult<User?>(id == User.Id ? User : null);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            LastSaved = new SavedProfile(User.FullName, User.PhoneNumber, User.UserDescription, User.Image);
            return Task.FromResult(1);
        }

        public Task<User?> GetByEmailAsync(string email) => throw new NotSupportedException();
        public Task<User?> GetByPhoneNumberAsync(string phoneNumber) => throw new NotSupportedException();
        public Task<bool> ExistsBySlugAsync(string slug) => throw new NotSupportedException();
        public Task<UserModel?> GetUserWithDetailsAsync(int userId) => throw new NotSupportedException();
        public Task<List<User>> GetAllAsync(Expression<Func<User, bool>>? predicate = null) => throw new NotSupportedException();
        public IQueryable<User> Query(Expression<Func<User, bool>>? predicate = null) => throw new NotSupportedException();
        public Task AddAsync(User entity) => throw new NotSupportedException();
        public Task UpdateAsync(User entity) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int id) => throw new NotSupportedException();
        public Task<List<int>> GetAllIds() => throw new NotSupportedException();
    }

    private sealed class FakeUploader : IUploaderService
    {
        public const string StoredPath = "uploads/users/0/new.webp";

        public List<string?> Deleted { get; } = [];
        public List<string> Uploaded { get; } = [];
        public bool UploadFails { get; init; }

        public Task<string?> UploadAsWebp(UploadDTO request)
        {
            Uploaded.Add(request.Path);
            return Task.FromResult<string?>(UploadFails ? null : StoredPath);
        }

        public Task DeleteStoredFile(string? storedPath, string fallbackDirectory)
        {
            Deleted.Add(storedPath);
            return Task.CompletedTask;
        }

        public Task<string?> UploadAsPng(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpeg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadVideo(UploadDTO request) => UploadAsWebp(request);
        public Task DeleteFile(DeleteDTO request) => Task.CompletedTask;
    }
}
