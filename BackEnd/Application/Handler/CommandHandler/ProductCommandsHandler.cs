using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.ValueObjects;
 

namespace Application.Handler.CommandHandler
{
    public class ProductCommandHandler(IProductRepository _repository, IHttpContextAccessor _accessor, IUploaderService _uploaderService) : 
        IRequestHandler<CreateProductCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateProductCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveProductCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteProductCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var existingProducts = await _repository.ExistsByNameAndBrandAsync(request.Name, request.BrandId);
            if (existingProducts)
            {
                return ServiceResult<IdDto>.Failed("محصولی با همین نام و برند از قبل وجود دارد.");
            }

            ProductDimensions dim= new ProductDimensions(request.Width??0, request.Height ?? 0, request.Depth ?? 0, request.Weight ?? 0);
            var product = Product.Create(
                name: request.Name,
                description: request.Description,
                categoryId: request.CategoryId,
                brandId: request.BrandId,
                dimensions: dim,
                currentUserId: userId.Value,
                slug: request.Slug,
                seoTitleFa: request.SeoTitleFa,
                seoTitleEn: request.SeoTitleEn,
                metaDescriptionFa: request.MetaDescriptionFa,
                metaDescriptionEn: request.MetaDescriptionEn
            );
            
            await _repository.AddAsync(product);
            await _repository.SaveChangesAsync(cancellationToken);

            // Guarantee uniqueness after Id is assigned.
            var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
                ? OnlineShop.Domain.Common.SlugHelper.Generate(request.Name, product.Id)
                : OnlineShop.Domain.Common.SlugHelper.Generate(request.Slug, product.Id);
            if (!string.Equals(product.Slug, baseSlug, StringComparison.OrdinalIgnoreCase)
                || await _repository.ExistsBySlugAsync(product.Slug, product.Id))
            {
                product.EnsureSlug(product.Id);
                await _repository.SaveChangesAsync(cancellationToken);
            } 
            //if (request.Images != null && request.Images.Any())
            //{
            //    for (int i = 0; i < request.Images.Count; i++)
            //    {
            //        var file = request.Images[i];
            //        var isMain = request.IsMainImages?.ElementAtOrDefault(i) ?? false;

            //        var uploadDto = new UploadDTO
            //        {
            //            File = file,
            //            Path = UploadPaths.Products(product.Id)
            //        };

            //        var imageUrl = await _uploaderService.UploadAsWebp(uploadDto);

            //        var productImage = ProductImage.Create(
            //            productId: product.Id,
            //            imageUrl: imageUrl,
            //            isMain: isMain,
            //            currentUserId: userId.Value
            //        );

            //        product.AddImage(productImage, userId.Value);
            //    }

            //    await _repository.SaveChangesAsync(cancellationToken);
            //}

            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return ServiceResult<IdDto>.Failed("محصول موردنظر پیدا نشد.");



            //List<ProductImage>? images = null;

            //if (request.Images != null && request.Images.Any())
            //{
            //    images = new List<ProductImage>();
            //    foreach (var imgDto in request.Images)
            //    {
            //        var uploadDto = new UploadDTO
            //        {
            //            File = imgDto.ProductImageFile,
            //            Path = UploadPaths.Products(product.Id)
            //        };

            //        var imageUrl = await _uploaderService.UploadAsWebp(uploadDto);

            //        var productImage = ProductImage.Create(
            //            productId: product.Id,
            //            imageUrl: imageUrl,
            //            isMain: imgDto.IsMain,
            //            currentUserId: userId.Value
            //        );

            //        images.Add(productImage);
            //    }
            //}
            //var dim = new ProductDimensions(request.Dimensions.Width, request.Dimensions.Height, request.Dimensions.Depth, request.Dimensions.Weight);
            //var specifications = request.Specifications?.Select(dto =>
            //ProductSpecification.Create(product.Id, dto.Key, dto.Value)).ToList() ?? new List<ProductSpecification>();

            ProductDimensions dim = new ProductDimensions(
                request.Width ?? product.Dimensions.Width,
                request.Height ?? product.Dimensions.Height,
                request.Depth ?? product.Dimensions.Depth,
                request.Weight ?? product.Dimensions.Weight);

            product.Update(
                currentUserId: userId.Value,
                name: request.Name,
                description: request.Description,
                categoryId: request.CategoryId,
                brandId: request.BrandId,
                dimensions: dim,
                slug: request.Slug,
                seoTitleFa: request.SeoTitleFa,
                seoTitleEn: request.SeoTitleEn,
                metaDescriptionFa: request.MetaDescriptionFa,
                metaDescriptionEn: request.MetaDescriptionEn
                );

            if (!string.IsNullOrWhiteSpace(product.Slug)
                && await _repository.ExistsBySlugAsync(product.Slug, product.Id))
            {
                product.EnsureSlug(product.Id);
            }

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return ServiceResult<IdDto>.Failed("محصول موردنظر پیدا نشد.");

            product.SetActive(request.IsActive, userId.Value);

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                return ServiceResult<IdDto>.Failed("محصول موردنظر پیدا نشد.");

            product.Delete(userId.Value);

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = product.Id });
        }
    }
    
}
