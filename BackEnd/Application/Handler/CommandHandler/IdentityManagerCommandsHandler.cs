using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;
using System.Security.Claims;
using static Application.Commands.IdentityManagerCommands;



public class UserCommandHandler(
    IUserRepository _repo,
    IUploaderService _uploaderService,
    IPasswordHasher<User> _passwordHasher,
    IJwtService _jwtService,
    IRefreshTokenRepository _refreshTokenRepo,
    IRoleRepository _roleRepo,
    IHttpContextAccessor _accessor)
    : IRequestHandler<CreateUserCommand, ServiceResult<IdDto>>,
IRequestHandler<LoginCommand, ServiceResult<LoginDto>>,
IRequestHandler<RefreshTokenCommand, ServiceResult<LoginDto>>,
IRequestHandler<ChangeUserPasswordByAdminCommand, ServiceResult<IdDto>>,
IRequestHandler<ChangeUserPasswordCommand, ServiceResult<IdDto>>,
IRequestHandler<SetUserRoleCommand, ServiceResult<IdDto>>,
IRequestHandler<ActiveUserCommand, ServiceResult<IdDto>>,
IRequestHandler<DeleteUserCommand, ServiceResult<IdDto>>

{
    public async Task<ServiceResult<IdDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {

        var existingUser = await _repo.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return ServiceResult<IdDto>.Failed("حسابی با این ایمیل از قبل وجود دارد.");


        var user = User.Create(
                fullName: request.FullName,
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                userDescription: request.UserDescription
                );
        await MakeSlugUniqueAsync(user);
        // The password goes in with the first save, so the account is never stored without one,
        // whatever happens to the profile image. No one is signed in yet: 0, as for anonymous contact requests.
        var hashedPassword = _passwordHasher.HashPassword(user, request.Password.Trim());
        user.ChangePassword(hashedPassword, currentUserId: 0);
        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync(cancellationToken);

        if (request.UserImageFile is not null && request.UserImageFile.Length > 0)
        {
            var uploadDto = new UploadDTO
            {
                File = request.UserImageFile,
                Path = UploadPaths.Users(user.Id)
            };
            string? imageUrl;
            try
            {
                imageUrl = await _uploaderService.UploadAsWebp(uploadDto);
            }
            catch
            {
                // All or nothing: remove the account (a hard delete) so the visitor can register again with
                // the same email, and let the error reach the client as before.
                await _repo.DeleteAsync(user.Id);
                throw;
            }
            if (UploadPaths.IsStoredPath(imageUrl))
                user.SetImage(imageUrl!, user.Id);
        }
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });

    }
    /// <summary>
    /// The slug comes from the name, which two customers can share, while /suppliers/{slug} has to
    /// stay unique. The first one keeps the clean address; the next ones get a number.
    /// </summary>
    private async Task MakeSlugUniqueAsync(User user)
    {
        if (!await _repo.ExistsBySlugAsync(user.Slug))
            return;

        // SlugHelper cuts at 180 characters: leave room, or a long name would lose its number.
        var stem = user.Slug.Length > 170 ? user.Slug[..170] : user.Slug;
        var suffix = 2;
        while (await _repo.ExistsBySlugAsync(SlugHelper.Generate(stem, suffix)))
            suffix++;

        user.SetSlug(SlugHelper.Generate(stem, suffix), currentUserId: 0);
    }

    public async Task<ServiceResult<LoginDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var ip = _accessor.HttpContext?.GetClientIp();
        var userAgent = _accessor.HttpContext?.GetUserAgent();
        // ===== بررسی نام کاربری و رمز عبور =====
        var user = await _repo.Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.EmailOrPhone || u.Email == request.EmailOrPhone, cancellationToken);
        if (user == null)
            return ServiceResult<LoginDto>.Failed("حسابی با این ایمیل یا شماره موبایل پیدا نشد.");
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password.Trim());
        if (result == PasswordVerificationResult.Failed)
            return ServiceResult<LoginDto>.Failed("رمز عبور اشتباه است.");

        // ===== Claims =====
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName ?? ""),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "")
        };

        // ===== تولید توکن‌ها =====
        var accessToken = _jwtService.GenerateToken(claims);
        var refreshTokenString = _jwtService.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            token: refreshTokenString,
            accessToken: accessToken,
            accessTokenExpiry: DateTime.UtcNow.AddMinutes(60),
            expiry: DateTime.UtcNow.AddDays(7),
            userId: user.Id,
            createdByIp: ip,
            createdByUserAgent: userAgent,
            currentUserId: user.Id
        );

        await _refreshTokenRepo.AddAsync(refreshToken);
        await _refreshTokenRepo.SaveChangesAsync(cancellationToken);

        // ===== خروجی =====
        return ServiceResult<LoginDto>.Ok(new LoginDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString
        });
    }
    public async Task<ServiceResult<LoginDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // ===== پیدا کردن توکن فعال =====
        var existingToken = await _refreshTokenRepo.GetByTokensAsync(request.AccessToken, request.RefreshToken);
        if (existingToken == null || !existingToken.IsActive())
            return ServiceResult<LoginDto>.Failed("نشست کاربری شما منقضی شده است؛ لطفاً دوباره وارد شوید.");

        // ===== revoke توکن قبلی و rotate =====
        existingToken.Revoke(replacedByToken: null, currentUserId: existingToken.UserId);

        // ===== Claims =====
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, existingToken.UserId.ToString()),
            new Claim(ClaimTypes.Name, existingToken.User.FullName ?? ""),
            new Claim(ClaimTypes.Email, existingToken.User.Email),
            new Claim(ClaimTypes.Role, existingToken.User.Role?.RoleName ?? "")
        };

        var newAccessToken = _jwtService.GenerateToken(claims);
        var newRefreshTokenString = _jwtService.GenerateRefreshToken();

        var newRefreshToken = RefreshToken.Create(
            token: newRefreshTokenString,
            accessToken: newAccessToken,
            accessTokenExpiry: DateTime.UtcNow.AddMinutes(60),
            expiry: DateTime.UtcNow.AddDays(7),
            userId: existingToken.UserId,
            createdByIp: request.Ip ?? "unknown",
            createdByUserAgent: request.UserAgent,
            currentUserId: existingToken.UserId
        );

        await _refreshTokenRepo.AddAsync(newRefreshToken);
        await _refreshTokenRepo.SaveChangesAsync(cancellationToken);

        return ServiceResult<LoginDto>.Ok(new LoginDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString
        });
    }
    public async Task<ServiceResult<IdDto>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();

        var user = await _repo.GetByIdAsync(request.Id);
        if (user == null)
            return ServiceResult<IdDto>.Unauthorized("حساب کاربری موردنظر پیدا نشد؛ لطفاً دوباره وارد شوید.");
        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.OldPassword);
        if (result == PasswordVerificationResult.Failed)
            return ServiceResult<IdDto>.Failed("رمز عبور فعلی را درست وارد نکردید.");
        var hashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
        user.ChangePassword(hashedPassword, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(ChangeUserPasswordByAdminCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var user = await _repo.GetByIdAsync(request.Id);
        if (user == null)
            return ServiceResult<IdDto>.Failed("کاربری پیدا نشد");
        var hashedPassword = _passwordHasher.HashPassword(user, request.NewPassword);
        user.ChangePassword(hashedPassword, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });

    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveUserCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var user = await _repo.GetByIdAsync(request.Id);
        if (user == null)
            return ServiceResult<IdDto>.Failed("کاربری پیدا نشد");

        user.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });

    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var user = await _repo.GetByIdAsync(request.Id);
        if (user == null)
            return ServiceResult<IdDto>.Failed("کاربری پیدا نشد");

        await _uploaderService.DeleteStoredFile(
            user.Image,
            UploadPaths.Users(user.Id));

        user.Delete(userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });

    }
    public async Task<ServiceResult<IdDto>> Handle(SetUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(request.UserId);
        if (user == null)
            return ServiceResult<IdDto>.Failed("کاربر موردنظر پیدا نشد.");
        var role = await _roleRepo.GetByIdAsync(request.RoleId);
        if (role == null)
            return ServiceResult<IdDto>.Failed("نقش کاربری موردنظر پیدا نشد.");
        user.SetRole(role, user.Id);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = user.Id });
    }
}

