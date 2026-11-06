using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

public class CategoryCommandHandler(
    ICategoryRepository _repo,
    IHttpContextAccessor _accessor,
    IUploaderService _uploaderService,
    IUnitOfWork _unitOfWork) :
    IRequestHandler<CreateCategoryCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateCategoryCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveCategoryCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteCategoryCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var category = Category.Create(
             request.PersianName,
             request.CategoryPersianDesc,
             request.EnglishName,
             request.CategoryEnglishDesc,
             request.IsShowInLanding,
             request.IsActive,
            parentCategoryId: request.ParentCategoryId,
            currentUserId: userId.Value,
            slug: request.Slug,
            seoTitleFa: request.SeoTitleFa,
            seoTitleEn: request.SeoTitleEn,
            metaDescriptionFa: request.MetaDescriptionFa,
            metaDescriptionEn: request.MetaDescriptionEn,
            faqJson: request.FaqJson
        );

        // The id names the upload folder, so the row is saved before the file; the
        // transaction takes it back if the file cannot be stored.
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _repo.AddAsync(category);
        await _repo.SaveChangesAsync(cancellationToken);
        category.EnsureSlug(category.Id);
        await _repo.SaveChangesAsync(cancellationToken);

        if (request.CategoryCover is not null)
        {
            var uploadDto = new UploadDTO
            {
                File = request.CategoryCover,
                Path = UploadPaths.Categories(category.Id)
            };

            var thumbnailUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(thumbnailUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر دسته‌بندی ناموفق بود");

            category.Update(
                persianName: null,
                persianDesc: null,
                englishName: null,
                englishDesc: null,
                imageUrl: thumbnailUrl,
                showInLanding: null,
                parentCategoryId: null,
                currentUserId: userId.Value);
        }

        if (request.IsActive.HasValue)
            category.SetActive(request.IsActive.Value, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = category.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var category = await _repo.GetByIdAsync(request.Id);
        if (category == null)
            return ServiceResult<IdDto>.Failed("دسته‌بندی موردنظر پیدا نشد.");

        string? thumbnailUrl = null;
        if (request.CategoryCover is not null)
        {
            var uploadDto = new UploadDTO
            {
                File = request.CategoryCover,
                Path = UploadPaths.Categories(category.Id)
            };

            thumbnailUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(thumbnailUrl))
                return ServiceResult<IdDto>.Failed("آپلود تصویر دسته‌بندی ناموفق بود");
        }

        var oldCover = category.ImageUrl;
        category.Update(
            persianName: request.PersianName,
            persianDesc: request.CategoryPersianDesc,
            englishName: request.EnglishName,
            englishDesc: request.CategoryEnglishDesc,
            imageUrl: thumbnailUrl,
            showInLanding: request.IsShowInLanding,
            parentCategoryId: request.ParentCategoryId,
            currentUserId: userId.Value,
            slug: request.Slug,
            seoTitleFa: request.SeoTitleFa,
            seoTitleEn: request.SeoTitleEn,
            metaDescriptionFa: request.MetaDescriptionFa,
            metaDescriptionEn: request.MetaDescriptionEn,
            faqJson: request.FaqJson
        );
        category.EnsureSlug(category.Id);

        if (request.IsActive.HasValue)
            category.SetActive(request.IsActive.Value, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        // Only now: a failed upload or save must leave the current cover in place.
        if (thumbnailUrl is not null)
            await _uploaderService.DeleteStoredFile(oldCover, UploadPaths.Categories(category.Id));

        return ServiceResult<IdDto>.Ok(new IdDto { Id = category.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var category = await _repo.GetByIdAsync(request.Id);
        if (category == null)
            return ServiceResult<IdDto>.Failed("دسته‌بندی موردنظر پیدا نشد.");

        category.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = category.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var category = await _repo.GetByIdAsync(request.Id);
        if (category == null)
            return ServiceResult<IdDto>.Failed("دسته‌بندی موردنظر پیدا نشد.");

        await _uploaderService.DeleteStoredFile(
            category.ImageUrl,
            UploadPaths.Categories(category.Id));

        category.Delete(userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = category.Id });
    }
}
