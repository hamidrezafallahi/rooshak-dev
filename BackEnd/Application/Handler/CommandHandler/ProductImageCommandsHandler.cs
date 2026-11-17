using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

/// <summary>
/// Product gallery commands. Every image is managed on its own: adding never removes
/// the images a product already has, and a product with images always keeps exactly one main image.
/// </summary>
public class AddProductImageCommandHandler(
        IProductImageRepository _repository,
        IProductRepository _productRepo,
        IHttpContextAccessor _accessor,
        IUploaderService _uploaderService) :
    IRequestHandler<AddProductImageCommand, ServiceResult<IdDto>>,
    IRequestHandler<ReplaceProductImageFileCommand, ServiceResult<IdDto>>,
    IRequestHandler<SetMainProductImageCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveProductImageCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteProductImageCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var product = _productRepo.Query(p => p.Id == request.ProductId).FirstOrDefault();
        if (product == null)
            return ServiceResult<IdDto>.Failed("محصول پیدا نشد");

        if (request.Images == null || !request.Images.Any())
            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });

        // Store every file before touching the gallery: a failed upload leaves the product as it was.
        var uploaded = new List<(string Url, string OriginalUrl, bool IsMain)>();
        for (int i = 0; i < request.Images.Count; i++)
        {
            var file = request.Images[i];
            var isMain = request.IsMainImages?.ElementAtOrDefault(i) ?? false;

            var imageUrl = await _uploaderService.UploadAsWebp(new UploadDTO
            {
                File = file,
                Path = UploadPaths.Products(product.Id)
            });
            if (!UploadPaths.IsStoredPath(imageUrl))
                return ServiceResult<IdDto>.Failed($"آپلود تصویر «{file.FileName}» ناموفق بود");

            var originalUrl = await _uploaderService.UploadOriginalImage(new UploadDTO
            {
                File = file,
                Path = UploadPaths.Products(product.Id)
            });
            if (!UploadPaths.IsStoredPath(originalUrl))
                return ServiceResult<IdDto>.Failed($"ذخیرهٔ فایل اصلی «{file.FileName}» ناموفق بود");

            uploaded.Add((imageUrl!, originalUrl!, isMain));
        }

        var existing = _repository.Query(pi => pi.ProductId == product.Id).ToList();

        // Only one new image may claim the main role; with none claimed, the first one
        // becomes main when the product has no main image yet.
        var mainIndex = uploaded.FindIndex(u => u.IsMain);
        if (mainIndex < 0 && !existing.Any(img => img.IsMain))
            mainIndex = 0;

        if (mainIndex >= 0)
            foreach (var img in existing.Where(img => img.IsMain))
                img.SetMain(false, userId.Value);

        for (int i = 0; i < uploaded.Count; i++)
        {
            product.AddImage(
                ProductImage.Create(
                    productId: product.Id,
                    imageUrl: uploaded[i].Url,
                    isMain: i == mainIndex,
                    currentUserId: userId.Value,
                    originalUrl: uploaded[i].OriginalUrl),
                userId.Value);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ReplaceProductImageFileCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        if (request.Image == null || request.Image.Length == 0)
            return ServiceResult<IdDto>.Failed("فایل تصویر انتخاب نشده است");

        var image = await _repository.GetByIdAsync(request.Id);
        if (image == null || image.IsDeleted)
            return ServiceResult<IdDto>.Failed("تصویر پیدا نشد");

        var imageUrl = await _uploaderService.UploadAsWebp(new UploadDTO
        {
            File = request.Image,
            Path = UploadPaths.Products(image.ProductId)
        });
        if (!UploadPaths.IsStoredPath(imageUrl))
            return ServiceResult<IdDto>.Failed($"آپلود تصویر «{request.Image.FileName}» ناموفق بود");

        var originalUrl = await _uploaderService.UploadOriginalImage(new UploadDTO
        {
            File = request.Image,
            Path = UploadPaths.Products(image.ProductId)
        });
        if (!UploadPaths.IsStoredPath(originalUrl))
            return ServiceResult<IdDto>.Failed($"ذخیرهٔ فایل اصلی «{request.Image.FileName}» ناموفق بود");

        var previousUrl = image.ImageUrl;
        var previousOriginal = image.OriginalUrl;
        image.UpdateUrl(imageUrl!, userId.Value, originalUrl);
        await _repository.SaveChangesAsync(cancellationToken);

        // The old files go only after the new paths are saved.
        await _uploaderService.DeleteStoredFile(previousUrl, UploadPaths.Products(image.ProductId));
        if (!string.IsNullOrWhiteSpace(previousOriginal))
            await _uploaderService.DeleteStoredFile(previousOriginal, UploadPaths.Products(image.ProductId));

        return ServiceResult<IdDto>.Ok(new IdDto { Id = image.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(SetMainProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var image = await _repository.GetByIdAsync(request.Id);
        if (image == null || image.IsDeleted)
            return ServiceResult<IdDto>.Failed("تصویر پیدا نشد");

        var siblings = _repository.Query(pi => pi.ProductId == image.ProductId).ToList();
        foreach (var img in siblings.Where(img => img.IsMain && img.Id != image.Id))
            img.SetMain(false, userId.Value);

        if (!image.IsMain)
            image.SetMain(true, userId.Value);

        // The storefront shows only active main images, so the chosen one must be visible.
        if (!image.IsActive)
            image.SetActive(true, userId.Value);

        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = image.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var image = await _repository.GetByIdAsync(request.Id);
        if (image == null)
            return ServiceResult<IdDto>.Failed("تصویر پیدا نشد");

        image.SetActive(request.IsActive, userId.Value);
        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = image.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var image = await _repository.GetByIdAsync(request.Id);
        if (image == null || image.IsDeleted)
            return ServiceResult<IdDto>.Failed("تصویر پیدا نشد");

        var wasMain = image.IsMain;
        image.Delete(userId.Value);
        if (wasMain)
            image.SetMain(false, userId.Value);

        // Removing the main image hands the role to the oldest remaining one (active first).
        if (wasMain)
        {
            var successor = _repository
                .Query(pi => pi.ProductId == image.ProductId && pi.Id != image.Id)
                .OrderByDescending(pi => pi.IsActive)
                .ThenBy(pi => pi.Id)
                .FirstOrDefault();
            successor?.SetMain(true, userId.Value);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await _uploaderService.DeleteStoredFile(
            image.ImageUrl,
            UploadPaths.Products(image.ProductId));
        if (!string.IsNullOrWhiteSpace(image.OriginalUrl))
            await _uploaderService.DeleteStoredFile(image.OriginalUrl, UploadPaths.Products(image.ProductId));

        return ServiceResult<IdDto>.Ok(new IdDto { Id = image.Id });
    }
}
