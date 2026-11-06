using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using Services.Services.Uploader.DTO;

public class BlogCommandHandler(
            IBlogRepository _blogRepository,
            IBlogContentQualityService _qualityService,
            IHttpContextAccessor _accessor,
            IUploaderService _uploaderService,
            IUnitOfWork _unitOfWork) :
        IRequestHandler<CreateBlogCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateBlogCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteBlogCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveBlogCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var isAiPipeline = string.Equals(request.Source, "ai-pipeline", StringComparison.OrdinalIgnoreCase)
            || request.IsDraft;

        if (isAiPipeline)
        {
            var quality = await _qualityService.ValidateAsync(new BlogContentQualityRequestDto
            {
                TitleFa = request.TitleFa,
                IntroFa = request.IntroFa,
                ContentFa = request.ContentFa,
                ConclusionFa = request.ConclusionFa,
                ExcerptFa = request.ExcerptFa,
                MetaDescriptionFa = request.MetaDescriptionFa,
                MetaKeywordsFa = request.MetaKeywordsFa,
                TitleEn = request.TitleEn,
                IntroEn = request.IntroEn,
                ContentEn = request.ContentEn,
                ConclusionEn = request.ConclusionEn,
                ExcerptEn = request.ExcerptEn,
                MetaDescriptionEn = request.MetaDescriptionEn,
                MetaKeywordsEn = request.MetaKeywordsEn,
                Slug = request.Slug,
            }, cancellationToken);

            if (!quality.IsValid)
                return ServiceResult<IdDto>.Failed(string.Join(" | ", quality.Errors));
        }
        else
        {
            var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(slug))
            {
                var exists = await _blogRepository.Query(b => !b.IsDeleted && b.Slug == slug)
                    .AnyAsync(cancellationToken);
                if (exists)
                    return ServiceResult<IdDto>.Failed($"مقاله‌ای با نامک «{slug}» از قبل وجود دارد؛ نامک دیگری انتخاب کنید.");
            }
        }

        var blog = Blog.Create(
             titleFa: request.TitleFa.Trim(),
             introFa: request.IntroFa,
             contentFa: request.ContentFa,
             conclusionFa: request.ConclusionFa,
             excerptFa: request.ExcerptFa,
             metaDescriptionFa: request.MetaDescriptionFa,
             metaKeywordsFa: request.MetaKeywordsFa,
             titleEn: string.IsNullOrWhiteSpace(request.TitleEn) ? request.TitleFa.Trim() : request.TitleEn.Trim(),
             introEn: string.IsNullOrWhiteSpace(request.IntroEn) ? request.IntroFa : request.IntroEn,
             contentEn: string.IsNullOrWhiteSpace(request.ContentEn) ? request.ContentFa : request.ContentEn,
             conclusionEn: string.IsNullOrWhiteSpace(request.ConclusionEn) ? request.ConclusionFa : request.ConclusionEn,
             excerptEn: request.ExcerptEn,
             metaDescriptionEn: request.MetaDescriptionEn,
             metaKeywordsEn: request.MetaKeywordsEn,
             slug: request.Slug,
             thumbnailFile: null,
             authorId: request.AuthorId ?? userId.Value,
             currentUserId: userId.Value
             );

        // The id names the upload folder, so the row is saved before the file; the
        // transaction takes it back if the file cannot be stored.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (request.IsDraft || isAiPipeline)
            blog.SetActive(false, userId.Value);

        await _blogRepository.AddAsync(blog);
        await _blogRepository.SaveChangesAsync(cancellationToken);

        if (request.ThumbnailFile is not null)
        {
            var uploadDto = new UploadDTO
            {
                File = request.ThumbnailFile,
                Path = UploadPaths.Blogs(blog.Id)
            };

            var thumbnailUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(thumbnailUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر بلاگ ناموفق بود");

            blog.UpdateFile(currentUserId: userId.Value, thumbnailFile: thumbnailUrl);
        }

        await _blogRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = blog.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return ServiceResult<IdDto>.Failed("بلاگ پیدا نشد");

        string? thumbnailUrl = null;
        if (request.ThumbnailFile is not null)
        {
            var uploadDto = new UploadDTO
            {
                File = request.ThumbnailFile,
                Path = UploadPaths.Blogs(blog.Id)
            };

            thumbnailUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(thumbnailUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر بلاگ ناموفق بود");
        }

        var oldThumbnail = blog.ThumbnailFile;
        blog.Update(
            currentUserId: userId.Value,
            titleFa: string.IsNullOrWhiteSpace(request.TitleFa) ? null : request.TitleFa,
            introFa: string.IsNullOrWhiteSpace(request.IntroFa) ? null : request.IntroFa,
            contentFa: string.IsNullOrWhiteSpace(request.ContentFa) ? null : request.ContentFa,
            conclusionFa: string.IsNullOrWhiteSpace(request.ConclusionFa) ? null : request.ConclusionFa,
            excerptFa: string.IsNullOrWhiteSpace(request.ExcerptFa) ? null : request.ExcerptFa,
            metaDescriptionFa: string.IsNullOrWhiteSpace(request.MetaDescriptionFa) ? null : request.MetaDescriptionFa,
            metaKeywordsFa: string.IsNullOrWhiteSpace(request.MetaKeywordsFa) ? null : request.MetaKeywordsFa,
            titleEn: string.IsNullOrWhiteSpace(request.TitleEn) ? null : request.TitleEn,
            introEn: string.IsNullOrWhiteSpace(request.IntroEn) ? null : request.IntroEn,
            contentEn: string.IsNullOrWhiteSpace(request.ContentEn) ? null : request.ContentEn,
            conclusionEn: string.IsNullOrWhiteSpace(request.ConclusionEn) ? null : request.ConclusionEn,
            excerptEn: string.IsNullOrWhiteSpace(request.ExcerptEn) ? null : request.ExcerptEn,
            metaDescriptionEn: string.IsNullOrWhiteSpace(request.MetaDescriptionEn) ? null : request.MetaDescriptionEn,
            metaKeywordsEn: string.IsNullOrWhiteSpace(request.MetaKeywordsEn) ? null : request.MetaKeywordsEn,
            slug: string.IsNullOrWhiteSpace(request.Slug) ? null : request.Slug,
            thumbnailFile: string.IsNullOrWhiteSpace(thumbnailUrl) ? null : thumbnailUrl,
            authorId: request.AuthorId ?? null
            );

        await _blogRepository.SaveChangesAsync(cancellationToken);

        // Only now: a failed upload or save must leave the current image in place.
        if (thumbnailUrl is not null)
            await _uploaderService.DeleteStoredFile(oldThumbnail, UploadPaths.Blogs(blog.Id));

        return ServiceResult<IdDto>.Ok(new IdDto { Id = blog.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return ServiceResult<IdDto>.Failed("بلاگ پیدا نشد");

        await _uploaderService.DeleteStoredFile(
            blog.ThumbnailFile,
            UploadPaths.Blogs(blog.Id));

        blog.Delete(userId.Value);
        await _blogRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = blog.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveBlogCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var blog = await _blogRepository.GetByIdAsync(request.Id);
        if (blog == null)
            return ServiceResult<IdDto>.Failed("بلاگ پیدا نشد");

        blog.SetActive(request.IsActive, userId.Value);
        await _blogRepository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = blog.Id });
    }
}
