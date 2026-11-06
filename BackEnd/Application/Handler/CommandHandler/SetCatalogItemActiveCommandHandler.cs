using OnlineShop.Domain.Common;
using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class SetCatalogItemActiveCommandHandler(
        IProductRepository productRepository,
        IProductOfferRepository offerRepository,
        IProductImageRepository imageRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor accessor)
        : IRequestHandler<SetCatalogItemActiveCommand, ServiceResult<CatalogItemActiveDto>>
    {
        public async Task<ServiceResult<CatalogItemActiveDto>> Handle(
            SetCatalogItemActiveCommand request,
            CancellationToken cancellationToken)
        {
            var userId = accessor.HttpContext?.GetUserId();
            if (userId == null)
                return ServiceResult<CatalogItemActiveDto>.Unauthorized();

            if (request.Id <= 0)
                return ServiceResult<CatalogItemActiveDto>.Failed("شناسهٔ محصول ارسال نشده است.");

            var product = await productRepository.GetByIdAsync(request.Id);
            if (product == null)
                return ServiceResult<CatalogItemActiveDto>.Failed("محصول موردنظر پیدا نشد.");

            var offers = await offerRepository.GetAllAsync(o => o.ProductId == product.Id);
            if (request.IsActive && offers.Count == 0)
                return ServiceResult<CatalogItemActiveDto>.Failed("این محصول هیچ پیشنهاد فروشی ندارد و نمی‌توان آن را منتشر کرد.");

            var images = await imageRepository.GetAllAsync(i => i.ProductId == product.Id);

            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                product.SetActive(request.IsActive, userId.Value);
                foreach (var offer in offers)
                    offer.SetActive(request.IsActive, userId.Value);
                foreach (var image in images)
                    image.SetActive(request.IsActive, userId.Value);

                await productRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ServiceResult<CatalogItemActiveDto>.Ok(new CatalogItemActiveDto
                {
                    ProductId = product.Id,
                    IsActive = request.IsActive,
                    OfferCount = offers.Count,
                    ImageCount = images.Count
                });
            }
            catch (DomainException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ServiceResult<CatalogItemActiveDto>.Failed(ex.Message, ex.Code);
            }
            catch
            {
                // Nothing was kept; the error middleware reports what happened.
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
