using System.Linq.Expressions;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using Xunit;
using static Application.Commands.IdentityManagerCommands;

namespace Application.Tests;

public class RegisterCommandHandlerTests
{
    private const string Password = "s3cret-pass";

    [Fact]
    public async Task Failed_profile_image_leaves_no_account_and_the_email_can_register_again()
    {
        var repo = new FakeUserRepository();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateHandler(repo, new FakeUploader(fails: true))
                .Handle(Command(withImage: true), CancellationToken.None));

        Assert.Empty(repo.Users);
        Assert.DoesNotContain(string.Empty, repo.SavedPasswords);

        var retry = await CreateHandler(repo, new FakeUploader(fails: false))
            .Handle(Command(withImage: false), CancellationToken.None);

        Assert.True(retry.IsSuccess);
        Assert.Single(repo.Users);
    }

    [Fact]
    public async Task Account_is_never_saved_without_a_password()
    {
        var repo = new FakeUserRepository();

        var result = await CreateHandler(repo, new FakeUploader(fails: false))
            .Handle(Command(withImage: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(repo.SavedPasswords);
        Assert.DoesNotContain(string.Empty, repo.SavedPasswords);
        var user = Assert.Single(repo.Users);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, Password));
        Assert.Equal(FakeUploader.StoredPath, user.Image);
    }

    [Fact]
    public async Task Customers_with_the_same_name_can_all_register()
    {
        var repo = new FakeUserRepository();

        foreach (var (email, expectedSlug) in new[]
        {
            ("first@example.com", "sara-ahmadi"),
            ("second@example.com", "sara-ahmadi-2"),
            ("third@example.com", "sara-ahmadi-3"),
        })
        {
            var command = Command(withImage: false);
            command.Email = email;

            var result = await CreateHandler(repo, new FakeUploader(fails: false))
                .Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedSlug, repo.Users[^1].Slug);
        }
    }

    [Fact]
    public async Task A_name_longer_than_the_slug_limit_still_gets_its_own_slug()
    {
        var repo = new FakeUserRepository();

        foreach (var email in new[] { "first@example.com", "second@example.com", "third@example.com" })
        {
            var command = Command(withImage: false);
            command.Email = email;
            command.FullName = new string('a', 200);

            await CreateHandler(repo, new FakeUploader(fails: false))
                .Handle(command, CancellationToken.None);
        }

        Assert.Equal(3, repo.Users.Select(u => u.Slug).Distinct().Count());
        Assert.All(repo.Users, user => Assert.True(user.Slug.Length <= 180));
    }

    private static UserCommandHandler CreateHandler(FakeUserRepository repo, FakeUploader uploader) =>
        new(repo, uploader, new PasswordHasher<User>(), null!, null!, null!, new HttpContextAccessor());

    private static CreateUserCommand Command(bool withImage) => new()
    {
        FullName = "Sara Ahmadi",
        Email = "sara@example.com",
        PhoneNumber = "09120000000",
        Password = Password,
        UserImageFile = withImage
            ? new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "UserImageFile", "avatar.jpg")
            : null,
    };

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];

        /// <summary>The password of every account in the store at each SaveChangesAsync call.</summary>
        public List<string> SavedPasswords { get; } = [];

        public Task<User?> GetByEmailAsync(string email) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<bool> ExistsBySlugAsync(string slug) =>
            Task.FromResult(Users.Any(u => u.Slug == slug));

        public Task AddAsync(User entity)
        {
            Users.Add(entity);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SavedPasswords.AddRange(Users.Select(u => u.Password));
            return Task.FromResult(1);
        }

        // Like Repository<T>.DeleteAsync: a hard delete that saves on its own.
        public Task<bool> DeleteAsync(int id) => Task.FromResult(Users.RemoveAll(u => u.Id == id) > 0);

        public Task<User?> GetByPhoneNumberAsync(string phoneNumber) => throw new NotSupportedException();
        public Task<UserModel?> GetUserWithDetailsAsync(int userId) => throw new NotSupportedException();
        public Task<User?> GetByIdAsync(int id) => throw new NotSupportedException();
        public Task<List<User>> GetAllAsync(Expression<Func<User, bool>>? predicate = null) => throw new NotSupportedException();
        public IQueryable<User> Query(Expression<Func<User, bool>>? predicate = null) => throw new NotSupportedException();
        public Task UpdateAsync(User entity) => throw new NotSupportedException();
        public Task<List<int>> GetAllIds() => throw new NotSupportedException();
    }

    private sealed class FakeUploader(bool fails) : IUploaderService
    {
        public const string StoredPath = "uploads/users/0/avatar.webp";

        public Task<string?> UploadAsWebp(UploadDTO request) =>
            fails
                ? throw new InvalidOperationException("The image could not be decoded.")
                : Task.FromResult<string?>(StoredPath);

        public Task<string?> UploadAsPng(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpeg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadAsJpg(UploadDTO request) => UploadAsWebp(request);
        public Task<string?> UploadVideo(UploadDTO request) => UploadAsWebp(request);
        public Task DeleteFile(DeleteDTO request) => Task.CompletedTask;
        public Task DeleteStoredFile(string? storedPath, string fallbackDirectory) => Task.CompletedTask;
    }
}
