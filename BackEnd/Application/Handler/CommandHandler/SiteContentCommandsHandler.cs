using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

namespace Application.Handler.CommandHandler
{
    /// <summary>اسلایدر میانی صفحه‌ی اصلی؛ فایل‌ها در uploads/homeslides/{id} و جدا از بنر هیرو.</summary>
    public class HomeSlideCommandHandler(
        IHomeSlideRepository _repo,
        IHttpContextAccessor _accessor,
        IUploaderService _uploader,
        IUnitOfWork _unitOfWork) :
        IRequestHandler<CreateHomeSlideCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateHomeSlideCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveHomeSlideCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteHomeSlideCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateHomeSlideCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            if (request.ImageUrl is not { Length: > 0 })
                return ServiceResult<IdDto>.Failed("انتخاب عکس اسلاید الزامی است");

            HomeSlide slide;
            try
            {
                slide = HomeSlide.Create(request.LinkUrl, request.TitleFa, request.TitleEn, request.DisplayOrder, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            // The id names the upload folder, so the row is saved before the file; the
            // transaction takes it back if the file cannot be stored.
            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            await _repo.AddAsync(slide);
            await _repo.SaveChangesAsync(ct);

            var image = await Upload(request.ImageUrl, slide.Id);
            if (image is null)
                return ServiceResult<IdDto>.Failed("آپلود عکس اسلاید ناموفق بود");
            slide.SetImage(image, userId.Value);

            if (request.MobileImageUrl is { Length: > 0 })
            {
                var mobile = await Upload(request.MobileImageUrl, slide.Id);
                if (mobile is null)
                    return ServiceResult<IdDto>.Failed("آپلود عکس موبایل ناموفق بود");
                slide.SetMobileImage(mobile, userId.Value);
            }

            await _repo.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateHomeSlideCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var slide = await _repo.GetByIdAsync(request.Id);
            if (slide == null)
                return ServiceResult<IdDto>.Failed("اسلاید پیدا نشد");

            try
            {
                slide.Update(request.LinkUrl, request.TitleFa, request.TitleEn, request.DisplayOrder, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            var before = new[] { slide.ImageUrl, slide.MobileImageUrl };

            if (request.ImageUrl is { Length: > 0 })
            {
                var image = await Upload(request.ImageUrl, slide.Id);
                if (image is null)
                    return ServiceResult<IdDto>.Failed("آپلود عکس اسلاید ناموفق بود");
                slide.SetImage(image, userId.Value);
            }

            if (request.RemoveMobileImage)
                slide.SetMobileImage(null, userId.Value);
            if (request.MobileImageUrl is { Length: > 0 })
            {
                var mobile = await Upload(request.MobileImageUrl, slide.Id);
                if (mobile is null)
                    return ServiceResult<IdDto>.Failed("آپلود عکس موبایل ناموفق بود");
                slide.SetMobileImage(mobile, userId.Value);
            }

            await _repo.SaveChangesAsync(ct);

            // Only now: a failed upload or save must leave the slide with everything it had.
            var kept = new[] { slide.ImageUrl, slide.MobileImageUrl };
            foreach (var file in before.Except(kept).Where(f => !string.IsNullOrWhiteSpace(f)))
                await _uploader.DeleteStoredFile(file, UploadPaths.HomeSlides(slide.Id));

            return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveHomeSlideCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var slide = await _repo.GetByIdAsync(request.Id);
            if (slide == null)
                return ServiceResult<IdDto>.Failed("اسلاید پیدا نشد");

            slide.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteHomeSlideCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var slide = await _repo.GetByIdAsync(request.Id);
            if (slide == null)
                return ServiceResult<IdDto>.Failed("اسلاید پیدا نشد");

            foreach (var file in new[] { slide.ImageUrl, slide.MobileImageUrl }.Where(f => !string.IsNullOrWhiteSpace(f)))
                await _uploader.DeleteStoredFile(file, UploadPaths.HomeSlides(slide.Id));

            slide.Delete(userId.Value);
            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
        }

        private async Task<string?> Upload(IFormFile file, int slideId)
        {
            var stored = await _uploader.UploadAsWebp(new UploadDTO { File = file, Path = UploadPaths.HomeSlides(slideId) });
            return UploadPaths.IsStoredPath(stored) ? stored : null;
        }
    }

    /// <summary>بخش‌های ثابت صفحه‌ی درباره ما: فقط ویرایش متن و روشن/خاموش (کلیدها با migration ساخته می‌شوند).</summary>
    public class AboutSectionCommandHandler(IAboutSectionRepository _repo, IHttpContextAccessor _accessor) :
        IRequestHandler<UpdateAboutSectionCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveAboutSectionCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(UpdateAboutSectionCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var section = await _repo.GetByIdAsync(request.Id);
            if (section == null)
                return ServiceResult<IdDto>.Failed("بخش پیدا نشد");

            try
            {
                section.Update(new AboutSectionText(
                    request.EyebrowFa, request.EyebrowEn,
                    request.TitleFa, request.TitleEn,
                    request.BodyFa, request.BodyEn,
                    request.LinkTextFa, request.LinkTextEn,
                    request.LinkUrl), request.DisplayOrder, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = section.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveAboutSectionCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var section = await _repo.GetByIdAsync(request.Id);
            if (section == null)
                return ServiceResult<IdDto>.Failed("بخش پیدا نشد");

            section.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = section.Id });
        }
    }

    /// <summary>فونت‌های آپلودی ادمین؛ فایل در uploads/fonts/{id}.</summary>
    public class CustomFontCommandHandler(
        ICustomFontRepository _repo,
        IThemeSettingRepository _themes,
        IHttpContextAccessor _accessor,
        IUploaderService _uploader,
        IUnitOfWork _unitOfWork) :
        IRequestHandler<CreateCustomFontCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateCustomFontCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveCustomFontCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteCustomFontCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateCustomFontCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            if (request.FontFile is not { Length: > 0 })
                return ServiceResult<IdDto>.Failed("فایل فونت الزامی است");

            CustomFont font;
            try
            {
                font = CustomFont.Create(request.NameFa, request.NameEn, request.Category, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            await _repo.AddAsync(font);
            await _repo.SaveChangesAsync(ct);

            try
            {
                var path = await _uploader.UploadFont(new UploadDTO { File = request.FontFile, Path = UploadPaths.Fonts(font.Id) });
                if (!UploadPaths.IsStoredPath(path))
                    return ServiceResult<IdDto>.Failed("آپلود فایل فونت ناموفق بود");
                font.SetFile(path!, userId.Value);
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            await _repo.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = font.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateCustomFontCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var font = await _repo.GetByIdAsync(request.Id);
            if (font == null)
                return ServiceResult<IdDto>.Failed("فونت پیدا نشد");

            var oldFile = font.FileUrl;
            try
            {
                font.Update(request.NameFa, request.NameEn, request.Category, userId.Value);
                if (request.FontFile is { Length: > 0 })
                {
                    var path = await _uploader.UploadFont(new UploadDTO { File = request.FontFile, Path = UploadPaths.Fonts(font.Id) });
                    if (!UploadPaths.IsStoredPath(path))
                        return ServiceResult<IdDto>.Failed("آپلود فایل فونت ناموفق بود");
                    font.SetFile(path!, userId.Value);
                }
            }
            catch (DomainException ex)
            {
                return ServiceResult<IdDto>.Failed(ex.Message);
            }

            await _repo.SaveChangesAsync(ct);
            if (!string.IsNullOrWhiteSpace(oldFile) && oldFile != font.FileUrl)
                await _uploader.DeleteStoredFile(oldFile, UploadPaths.Fonts(font.Id));

            return ServiceResult<IdDto>.Ok(new IdDto { Id = font.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveCustomFontCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var font = await _repo.GetByIdAsync(request.Id);
            if (font == null)
                return ServiceResult<IdDto>.Failed("فونت پیدا نشد");

            if (!request.IsActive && await IsUsedByActiveTheme(font, ct))
                return ServiceResult<IdDto>.Failed("این فونت در تم فعال سایت استفاده شده است؛ ابتدا فونت تم را عوض کنید");

            font.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = font.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteCustomFontCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var font = await _repo.GetByIdAsync(request.Id);
            if (font == null)
                return ServiceResult<IdDto>.Failed("فونت پیدا نشد");

            if (await IsUsedByActiveTheme(font, ct))
                return ServiceResult<IdDto>.Failed("این فونت در تم فعال سایت استفاده شده است؛ ابتدا فونت تم را عوض کنید");

            if (!string.IsNullOrWhiteSpace(font.FileUrl))
                await _uploader.DeleteStoredFile(font.FileUrl, UploadPaths.Fonts(font.Id));

            font.Delete(userId.Value);
            await _repo.SaveChangesAsync(ct);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = font.Id });
        }

        private async Task<bool> IsUsedByActiveTheme(CustomFont font, CancellationToken ct)
        {
            var theme = await _themes.GetActiveAsync(ct);
            return theme is not null && (theme.FontFamily == font.Key || theme.HeadingFontFamily == font.Key);
        }
    }
}
