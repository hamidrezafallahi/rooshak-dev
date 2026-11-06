using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

public class AddProductImageCommandHandler(
        IProductImageRepository _repository,
        IProductRepository _productRepo,
        IHttpContextAccessor _accessor,
        IUploaderService _uploaderService) :
    IRequestHandler<AddProductImageCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveProductImageCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteProductImageCommand, ServiceResult<IdDto>>
{
    public async Task<ServiceResult<IdDto>> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var product = _productRepo.Query(p => p.Id == request.ProductId).FirstOrDefault();
        if (product == null)
            return ServiceResult<IdDto>.Failed("محصول پیدا نشد");

        if (request.Images == null || !request.Images.Any())
            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });

        // New files first: if one cannot be stored, the product keeps the images it has.
        var uploaded = new List<(string Url, bool IsMain)>();
        for (int i = 0; i < request.Images.Count; i++)
        {
            var file = request.Images[i];
            var isMain = request.IsMainImages?.ElementAtOrDefault(i) ?? false;

            var uploadDto = new UploadDTO
            {
                File = file,
                Path = UploadPaths.Products(product.Id)
            };

            var imageUrl = await _uploaderService.UploadAsWebp(uploadDto);
            if (!UploadPaths.IsStoredPath(imageUrl))
                return ServiceResult<IdDto>.Failed($"آپلود تصویر «{file.FileName}» ناموفق بود");

            uploaded.Add((imageUrl!, isMain));
        }

        var replaced = _repository.Query(pi => pi.ProductId == request.ProductId).ToList();
        foreach (var img in replaced)
            img.Delete(userId.Value);

        foreach (var (url, isMain) in uploaded)
        {
            product.AddImage(
                ProductImage.Create(
                    productId: product.Id,
                    imageUrl: url,
                    isMain: isMain,
                    currentUserId: userId.Value),
                userId.Value);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        foreach (var img in replaced)
            await _uploaderService.DeleteStoredFile(img.ImageUrl, UploadPaths.Products(product.Id));

        return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(ActiveProductImageCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Failed("Unauthorized");

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
            return ServiceResult<IdDto>.Failed("Unauthorized");

        var image = await _repository.GetByIdAsync(request.Id);
        if (image == null)
            return ServiceResult<IdDto>.Failed("تصویر پیدا نشد");

        await _uploaderService.DeleteStoredFile(
            image.ImageUrl,
            UploadPaths.Products(image.ProductId));

        image.Delete(userId.Value);
        await _repository.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = image.Id });
    }
}
