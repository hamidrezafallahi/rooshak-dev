using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class SpacialOfferCommandHandler(
            ISpecialOfferRepository _repo,
            IHttpContextAccessor _accessor) : 
        IRequestHandler<CreateSpecialOfferCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateSpecialOfferCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveSpecialOfferCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteSpecialOfferCommand, ServiceResult<IdDto>>

    {


        public async Task<ServiceResult<IdDto>> Handle(CreateSpecialOfferCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();


            var spacialOffer = SpecialOffer.Create(
             request.ProductOfferId,
             request.StartDate,
             request.EndDate,
            userId.Value,
            request.DiscountId,
            request.DisplayOrder
            );

            await _repo.AddAsync(spacialOffer);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = spacialOffer.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateSpecialOfferCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var spacialOffer = await _repo.GetByIdAsync(request.Id);
            if (spacialOffer == null)
                return ServiceResult<IdDto>.Failed("پیشنهاد ویژه پیدا نشد");
            spacialOffer.Update(
             userId.Value,
             request.ProductOfferId,
             request.StartDate,
             request.EndDate,
             request.DiscountId,
             request.DisplayOrder
            );

            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = spacialOffer.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveSpecialOfferCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var specialOffer = await _repo.GetByIdAsync(request.Id);
            if (specialOffer == null)
                return ServiceResult<IdDto>.Failed("پیشنهاد ویژه پیدا نشد");

            specialOffer.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = specialOffer.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteSpecialOfferCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var specialOffer = await _repo.GetByIdAsync(request.Id);
            if (specialOffer == null)
                return ServiceResult<IdDto>.Failed("پیشنهاد ویژه پیدا نشد");

            specialOffer.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = specialOffer.Id });
        }


    }

  
 
}
