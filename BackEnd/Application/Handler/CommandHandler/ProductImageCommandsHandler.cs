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
            return ServiceResult<IdDto>.Unauthorized();

        var product = _productRepo.Query(p => p.Id == request.ProductId).FirstOrDefault();
        if (product == null)
            return ServiceResult<IdDto>.Failed("محصول پیدا نشد");

        var productImages = _repository.Query(pi => pi.ProductId == request.ProductId).ToList();
        if (productImages.Count > 0)
        {
            foreach (var img in productImages)
            {
                await _uploaderService.DeleteStoredFile(
                    img.ImageUrl,
                    UploadPaths.Products(product.Id));
                img.Delete(userId.Value);
            }
        }

        if (request.Images != null && request.Images.Any())
        {
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
                    continue;

                var productImage = ProductImage.Create(
                    productId: product.Id,
                    imageUrl: imageUrl!,
                    isMain: isMain,
                    currentUserId: userId.Value
                );

                product.AddImage(productImage, userId.Value);
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }

        return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
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
