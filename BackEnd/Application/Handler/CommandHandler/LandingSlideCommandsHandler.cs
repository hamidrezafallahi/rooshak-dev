using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using Services.Services.Uploader.DTO;

public class SlideCommandHandler(
        ISlideRepository _slideRepository,
        IHttpContextAccessor _accessor,
        IUploaderService _uploaderService,
        IUnitOfWork _unitOfWork) :
    IRequestHandler<CreateSlideCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateSlideCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveSlideCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteSlideCommand, ServiceResult<IdDto>>,
    IRequestHandler<SetHeroBannerCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(CreateSlideCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        if (request.BannerUrl is null || request.BannerUrl.Length == 0)
            return ServiceResult<IdDto>.Failed("انتخاب تصویر بنر الزامی است");

        if (string.IsNullOrWhiteSpace(request.FirstUrl))
            return ServiceResult<IdDto>.Failed("آدرس صفحه مربوط به بنر الزامی است");

        // The id names the upload folder, so the row is saved before the file; the
        // transaction takes it back if the file cannot be stored.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var slide = Slide.Create(
                userId.Value,
                request.FirstUrl,
                request.SecondUrl,
                request.BannerTitle,
                request.BannerDescription
            );

            await _slideRepository.AddAsync(slide);
            await _slideRepository.SaveChangesAsync(cancellationToken);

            var uploadDto = new UploadDTO
            {
                File = request.BannerUrl,
                Path = UploadPaths.LandingSlides(slide.Id)
            };

            var bannerUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(bannerUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر بنر ناموفق بود");

            string? videoPath = null;
            if (request.VideoUrl is not null && request.VideoUrl.Length > 0)
            {
                videoPath = await _uploaderService.UploadVideo(new UploadDTO
                {
                    File = request.VideoUrl,
                    Path = UploadPaths.LandingSlides(slide.Id)
                });
                if (!UploadPaths.IsStoredPath(videoPath))
                    return ServiceResult<IdDto>.Failed("آپلود ویدیو ناموفق بود (فقط mp4/webm تا ۳۰ مگابایت)");
            }

            slide.Update(userId.Value, bannerUrl, null, null, null, null, videoPath);

            var mobileBanner = await UploadImageIfAny(request.MobileBannerUrl, slide.Id);
            if (request.MobileBannerUrl is { Length: > 0 } && mobileBanner is null)
                return ServiceResult<IdDto>.Failed("آپلود پوستر موبایل ناموفق بود");

            var mobileVideo = await UploadVideoIfAny(request.MobileVideoUrl, slide.Id);
            if (request.MobileVideoUrl is { Length: > 0 } && mobileVideo is null)
                return ServiceResult<IdDto>.Failed("آپلود ویدیوی موبایل ناموفق بود (فقط mp4/webm تا ۳۰ مگابایت)");

            slide.SetMobileMedia(userId.Value, mobileBanner, mobileVideo);
            await _slideRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
        }
        catch (ArgumentException ex)
        {
            return ServiceResult<IdDto>.Failed(ex.Message);
        }
    }

    public async Task<ServiceResult<IdDto>> Handle(UpdateSlideCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var slide = await _slideRepository.GetByIdAsync(request.Id);
        if (slide == null)
            return ServiceResult<IdDto>.Failed("slide پیدا نشد");

        var filesBefore = new[] { slide.BannerUrl, slide.VideoUrl, slide.MobileBannerUrl, slide.MobileVideoUrl };

        string? bannerUrl = null;
        if (request.BannerUrl is not null && request.BannerUrl.Length > 0)
        {
            var uploadDto = new UploadDTO
            {
                File = request.BannerUrl,
                Path = UploadPaths.LandingSlides(slide.Id)
            };

            bannerUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(bannerUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر بنر ناموفق بود");
        }

        string? videoPath = null;
        if (request.RemoveVideo && !string.IsNullOrWhiteSpace(slide.VideoUrl))
            slide.ClearVideo(userId.Value);
        if (request.VideoUrl is not null && request.VideoUrl.Length > 0)
        {
            videoPath = await _uploaderService.UploadVideo(new UploadDTO
            {
                File = request.VideoUrl,
                Path = UploadPaths.LandingSlides(slide.Id)
            });
            if (!UploadPaths.IsStoredPath(videoPath))
                return ServiceResult<IdDto>.Failed("آپلود ویدیو ناموفق بود (فقط mp4/webm تا ۳۰ مگابایت)");
        }

        // ---- نسخه‌ی موبایل ----
        var folder = UploadPaths.LandingSlides(slide.Id);
        if (request.RemoveMobileBanner && !string.IsNullOrWhiteSpace(slide.MobileBannerUrl))
            slide.ClearMobileBanner(userId.Value);
        if (request.RemoveMobileVideo && !string.IsNullOrWhiteSpace(slide.MobileVideoUrl))
            slide.ClearMobileVideo(userId.Value);

        string? newMobileBanner = null;
        if (request.MobileBannerUrl is { Length: > 0 })
        {
            newMobileBanner = await UploadImageIfAny(request.MobileBannerUrl, slide.Id);
            if (newMobileBanner is null)
                return ServiceResult<IdDto>.Failed("آپلود پوستر موبایل ناموفق بود");
        }

        string? newMobileVideo = null;
        if (request.MobileVideoUrl is { Length: > 0 })
        {
            newMobileVideo = await UploadVideoIfAny(request.MobileVideoUrl, slide.Id);
            if (newMobileVideo is null)
                return ServiceResult<IdDto>.Failed("آپلود ویدیوی موبایل ناموفق بود (فقط mp4/webm تا ۳۰ مگابایت)");
        }

        slide.SetMobileMedia(userId.Value, newMobileBanner, newMobileVideo);

        slide.Update(
         userId.Value,
         bannerUrl,
         request.FirstUrl,
         request.SecondUrl,
         request.BannerTitle,
         request.BannerDescription,
         videoPath
        );
        await _slideRepository.SaveChangesAsync(cancellationToken);

        // Only now: a failed upload or save must leave the slide with everything it had.
        var filesKept = new[] { slide.BannerUrl, slide.VideoUrl, slide.MobileBannerUrl, slide.MobileVideoUrl };
        foreach (var file in filesBefore.Except(filesKept).Where(f => !string.IsNullOrWhiteSpace(f)))
            await _uploaderService.DeleteStoredFile(file, folder);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveSlideCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var slide = await _slideRepository.GetByIdAsync(request.Id);
        if (slide == null)
            return ServiceResult<IdDto>.Failed("اسلاید پیدا نشد");

        slide.SetActive(request.IsActive, userId.Value);
        await _slideRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteSlideCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var slide = await _slideRepository.GetByIdAsync(request.Id);
        if (slide == null)
            return ServiceResult<IdDto>.Failed("اسلاید پیدا نشد");

        await _uploaderService.DeleteStoredFile(
            slide.BannerUrl,
            UploadPaths.LandingSlides(slide.Id));

        // ویدیوها و نسخه‌ی موبایل هم با اسلاید پاک می‌شوند.
        foreach (var stored in new[] { slide.VideoUrl, slide.MobileBannerUrl, slide.MobileVideoUrl })
        {
            if (!string.IsNullOrWhiteSpace(stored))
                await _uploaderService.DeleteStoredFile(stored, UploadPaths.LandingSlides(slide.Id));
        }

        slide.Delete(userId.Value);
        await _slideRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = slide.Id });
    }

    private async Task<string?> UploadImageIfAny(IFormFile? file, int slideId)
    {
        if (file is not { Length: > 0 }) return null;
        var stored = await _uploaderService.UploadAsWebp(new UploadDTO
        {
            File = file,
            Path = UploadPaths.LandingSlides(slideId)
        });
        return UploadPaths.IsStoredPath(stored) ? stored : null;
    }

    private async Task<string?> UploadVideoIfAny(IFormFile? file, int slideId)
    {
        if (file is not { Length: > 0 }) return null;
        var stored = await _uploaderService.UploadVideo(new UploadDTO
        {
            File = file,
            Path = UploadPaths.LandingSlides(slideId)
        });
        return UploadPaths.IsStoredPath(stored) ? stored : null;
    }

    public async Task<ServiceResult<IdDto>> Handle(SetHeroBannerCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var slides = await _slideRepository.Query(s => s.IsActive).ToListAsync();
        if (!slides.Any())
            return ServiceResult<IdDto>.Failed("اسلایدی یافت نشد");

        foreach (var slide in slides)
            slide.SetHero(slide.Id == request.Id, userId.Value);

        await _slideRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = request.Id });
    }
}
