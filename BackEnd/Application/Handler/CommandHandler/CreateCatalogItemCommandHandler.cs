using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Domain.ValueObjects;

namespace Application.Handler.CommandHandler
{
    public class CreateCatalogItemCommandHandler(
        IProductRepository productRepository,
        IProductOfferRepository offerRepository,
        IProductImageRepository imageRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor accessor)
        : IRequestHandler<CreateCatalogItemCommand, ServiceResult<CatalogItemIdsDto>>
    {
        public async Task<ServiceResult<CatalogItemIdsDto>> Handle(
            CreateCatalogItemCommand request,
            CancellationToken cancellationToken)
        {
            var userId = accessor.HttpContext?.GetUserId();
            if (userId == null)
                return ServiceResult<CatalogItemIdsDto>.Failed("Unauthorized");

            var validationError = Validate(request);
            if (validationError != null)
                return ServiceResult<CatalogItemIdsDto>.Failed(validationError);

            if (await productRepository.ExistsByNameAndBrandAsync(request.Name, request.BrandId))
                return ServiceResult<CatalogItemIdsDto>.Failed("A product with the same name and brand already exists.");

            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var dimensions = new ProductDimensions(
                    request.Width ?? 0,
                    request.Height ?? 0,
                    request.Depth ?? 0,
                    request.Weight ?? 0);

                var product = Product.Create(
                    name: request.Name,
                    description: request.Description,
                    categoryId: request.CategoryId,
                    brandId: request.BrandId,
                    dimensions: dimensions,
                    currentUserId: userId.Value,
                    slug: request.Slug,
                    seoTitleFa: request.SeoTitleFa,
                    seoTitleEn: request.SeoTitleEn,
                    metaDescriptionFa: request.MetaDescriptionFa,
                    metaDescriptionEn: request.MetaDescriptionEn);
                product.SetActive(false, userId.Value);

                await productRepository.AddAsync(product);
                await productRepository.SaveChangesAsync(cancellationToken);

                var baseSlug = string.IsNullOrWhiteSpace(request.Slug)
                    ? OnlineShop.Domain.Common.SlugHelper.Generate(request.Name, product.Id)
                    : OnlineShop.Domain.Common.SlugHelper.Generate(request.Slug, product.Id);
                if (!string.Equals(product.Slug, baseSlug, StringComparison.OrdinalIgnoreCase)
                    || await productRepository.ExistsBySlugAsync(product.Slug, product.Id))
                {
                    product.EnsureSlug(product.Id);
                    await productRepository.SaveChangesAsync(cancellationToken);
                }

                ProductImage? image = null;
                if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    image = ProductImage.Create(
                        productId: product.Id,
                        imageUrl: request.ImageUrl.Trim(),
                        isMain: request.ImageIsMain,
                        currentUserId: userId.Value);
                    image.SetActive(false, userId.Value);
                    await imageRepository.AddAsync(image);
                }

                var offer = ProductOffers.Create(
                    productId: product.Id,
                    supplierId: userId.Value,
                    basePrice: request.BasePrice,
                    inventory: request.Inventory,
                    currentUserId: userId.Value);
                offer.SetActive(false, userId.Value);

                await offerRepository.AddAsync(offer);
                await offerRepository.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return ServiceResult<CatalogItemIdsDto>.Ok(new CatalogItemIdsDto
                {
                    ProductId = product.Id,
                    OfferId = offer.Id,
                    ImageId = image?.Id
                });
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                return ServiceResult<CatalogItemIdsDto>.Failed(
                    "Catalog item could not be created as a complete Product+Offer unit.");
            }
        }

        private static string? Validate(CreateCatalogItemCommand request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return "Product name cannot be empty.";
            if (request.CategoryId <= 0)
                return "Category is required.";
            if (request.BrandId <= 0)
                return "Brand is required.";
            if (request.BasePrice <= 0)
                return "Offer base price must be greater than zero.";
            if (request.Inventory < 0)
                return "Offer inventory cannot be negative.";
            return null;
        }
    }
}
