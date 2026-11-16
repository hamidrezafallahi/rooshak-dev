using OnlineShop.Domain.Common;
using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class ThemeSettingCommandHandler(IThemeSettingRepository _repo, ICustomFontRepository _fonts, IHttpContextAccessor _accessor) :
        IRequestHandler<CreateThemeSettingCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateThemeSettingCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveThemeSettingCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteThemeSettingCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateThemeSettingCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            try
            {
                var customKeys = await ActiveCustomFontKeys(cancellationToken);
                var theme = ThemeSetting.Create(request.Name, ToPalette(request), userId.Value, request.FontFamily, request.HeadingFontFamily,
                    new ThemeTypography(request.BodyTextSize, request.BodyTextSizePx, request.HeadingTextSize, request.HeadingTextSizePx, request.BodyFontWeight, request.HeadingFontWeight),
                    customKeys.Contains);

                // تم تازه فعال نمی‌شود تا سایت ناخواسته عوض نشود؛ ادمین با «فعال‌سازی» آن را انتخاب می‌کند.
                theme.SetActive(false, userId.Value);

                await _repo.AddAsync(theme);
                await _repo.SaveChangesAsync(cancellationToken);
                return ServiceResult<IdDto>.Ok(new IdDto { Id = theme.Id });
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateThemeSettingCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var theme = await _repo.GetByIdAsync(request.Id);
            if (theme == null)
                return ServiceResult<IdDto>.Failed("تم یافت نشد");

            try
            {
                var customKeys = await ActiveCustomFontKeys(cancellationToken);
                theme.Update(request.Name, ToPalette(request), userId.Value, request.FontFamily, request.HeadingFontFamily,
                    new ThemeTypography(request.BodyTextSize, request.BodyTextSizePx, request.HeadingTextSize, request.HeadingTextSizePx, request.BodyFontWeight, request.HeadingFontWeight),
                    customKeys.Contains);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = theme.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveThemeSettingCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var theme = await _repo.GetByIdAsync(request.Id);
            if (theme == null)
                return ServiceResult<IdDto>.Failed("تم یافت نشد");

            if (request.IsActive)
            {
                // فقط یک تم فعال: بقیه خاموش می‌شوند.
                var others = await _repo.Query(t => t.IsActive && t.Id != theme.Id).ToListAsync(cancellationToken);
                foreach (var other in others)
                    other.SetActive(false, userId.Value);

                theme.SetActive(true, userId.Value);
            }
            else
            {
                var otherActive = await _repo.Query(t => t.IsActive && t.Id != theme.Id).AnyAsync(cancellationToken);
                if (!otherActive)
                    return ServiceResult<IdDto>.Failed("حداقل یک تم باید فعال باشد؛ ابتدا تم دیگری را فعال کنید");

                theme.SetActive(false, userId.Value);
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = theme.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteThemeSettingCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var theme = await _repo.GetByIdAsync(request.Id);
            if (theme == null)
                return ServiceResult<IdDto>.Failed("تم یافت نشد");

            if (theme.IsActive)
                return ServiceResult<IdDto>.Failed("تم فعال قابل حذف نیست؛ ابتدا تم دیگری را فعال کنید");

            theme.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = theme.Id });
        }

        /// <summary>کلیدهای فونت‌های آپلودی فعال (custom-{id})؛ فقط همین‌ها در تم پذیرفته می‌شوند.</summary>
        private async Task<HashSet<string>> ActiveCustomFontKeys(CancellationToken ct)
        {
            var ids = await _fonts.Query(f => f.IsActive && f.FileUrl != "").Select(f => f.Id).ToListAsync(ct);
            return ids.Select(CustomFont.KeyFor).ToHashSet(StringComparer.Ordinal);
        }

        private static ThemePalette ToPalette(CreateThemeSettingCommand c) => new(
            c.PrimaryColor, c.SecondaryColor, c.HighlightColor, c.NeutralColor,
            c.SuccessColor, c.ErrorColor, c.WarningColor, c.InfoColor,
            c.SurfaceColor, c.SurfaceMutedColor, c.BorderColor, c.TextColor, c.TextMutedColor);

        private static ThemePalette ToPalette(UpdateThemeSettingCommand c) => new(
            c.PrimaryColor, c.SecondaryColor, c.HighlightColor, c.NeutralColor,
            c.SuccessColor, c.ErrorColor, c.WarningColor, c.InfoColor,
            c.SurfaceColor, c.SurfaceMutedColor, c.BorderColor, c.TextColor, c.TextMutedColor);
    }
}
