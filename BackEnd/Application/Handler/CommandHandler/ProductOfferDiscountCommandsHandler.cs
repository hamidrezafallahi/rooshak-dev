using Application.Commands;
using Application.Common;
using Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;
 

namespace Application.Handler.CommandHandler
{
public class ProductOfferDiscountCommandHandler(
            IProductOfferDiscountRepository _repository,
            IProductOfferRepository _productOfferRepo,
            IDiscountRepository _discountRepo,
            IHttpContextAccessor _accessor)
        : 
        IRequestHandler<CreateProductOfferDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateProductOfferDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveProductOfferDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteProductOfferDiscountCommand, ServiceResult<IdDto>>
    {
        

        public async Task<ServiceResult<IdDto>> Handle(CreateProductOfferDiscountCommand request,CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var productOffer = await _productOfferRepo.GetByIdAsync(request.ProductOfferId);
            if (productOffer == null || productOffer.IsDeleted)
                return ServiceResult<IdDto>.Failed("پیشنهاد محصول موردنظر پیدا نشد.");

            if (productOffer.SupplierId != userId.Value)
                return ServiceResult<IdDto>.Failed("این پیشنهاد متعلق به شما نیست و اجازهٔ تغییر آن را ندارید.");

 
            var discount = await _discountRepo.GetByIdAsync(request.DiscountId);
            if (discount == null || discount.IsDeleted)
                return ServiceResult<IdDto>.Failed("تخفیف موردنظر پیدا نشد.");

            var exists = (await _repository
                .Query(pd => pd.ProductOfferId == request.ProductOfferId &&
                            pd.DiscountId == request.DiscountId &&
                            !pd.IsDeleted)
                .ToListAsync(cancellationToken))
                .Any();

            if (exists)
                return ServiceResult<IdDto>.Failed("این تخفیف قبلاً به این پیشنهاد اضافه شده است.");

            // ایجاد
            var productOfferDiscount = ProductOfferDiscount.Create(
                productOfferId: request.ProductOfferId, // ✅ اصلاح شده
                discountId: request.DiscountId,
                currentUserId: userId.Value
            );

            await _repository.AddAsync(productOfferDiscount);
            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = productOfferDiscount.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateProductOfferDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var productOfferDiscount = await _repository.GetByIdAsync(request.Id);
            if (productOfferDiscount == null)
                return ServiceResult<IdDto>.Failed("رابطه محصول-تخفیف پیدا نشد");
            var sameDiscount = _repository.Query(pod =>pod.IsActive && pod.DiscountId == request.DiscountId && pod.ProductOfferId == request.ProductOfferId).FirstOrDefault();
            if (sameDiscount != null)
            {
                sameDiscount.SetActive(false,userId.Value);
            }

            productOfferDiscount.Update(
            productOfferId: request.ProductOfferId,
            discountId: request.DiscountId,
            currentUserId: userId.Value
        );

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = productOfferDiscount.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveProductOfferDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var productOfferDiscount = await _repository.GetByIdAsync(request.Id);
            if (productOfferDiscount == null)
                return ServiceResult<IdDto>.Failed("رابطه محصول-تخفیف پیدا نشد");

            productOfferDiscount.SetActive(request.IsActive, userId.Value);

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = productOfferDiscount.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteProductOfferDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var productOfferDiscount = await _repository.GetByIdAsync(request.Id);
            if (productOfferDiscount == null)
                return ServiceResult<IdDto>.Failed("رابطه محصول-تخفیف پیدا نشد");

            productOfferDiscount.Delete(userId.Value);

            await _repository.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = productOfferDiscount.Id });
        }
    }
}