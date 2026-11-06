using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

 
    public class DiscountCommandHandler(IDiscountRepository _repo, IHttpContextAccessor _accessor) : 
        IRequestHandler<CreateDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveDiscountCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteDiscountCommand, ServiceResult<IdDto>>
    {

        public async Task<ServiceResult<IdDto>> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var discount = Discount.Create(
         title: request.Title,
         amount: request.Amount,      
         isPercent: request.IsPercent,
         startDate: request.StartDate,
         endDate: request.EndDate,
         priority:request.Priority,
         currentUserId: userId.Value
     );

            await _repo.AddAsync(discount);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var discount = await _repo.GetByIdAsync(request.Id);
            if (discount == null)
                return ServiceResult<IdDto>.Failed("تخفیف پیدا نشد");

            discount.Update(
      title: request.Title,
      amount: request.Amount,
      isPercent: request.IsPercent,
      startDate: request.StartDate,
      endDate: request.EndDate,
      priority: request.Priority,
      currentUserId: userId.Value
  );
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });
        }
    public async Task<ServiceResult<IdDto>> Handle(ActiveDiscountCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var discount = await _repo.GetByIdAsync(request.Id);
        if (discount == null)
            return ServiceResult<IdDto>.Failed("تخفیف پیدا نشد");

        discount.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });
    }

    public async Task<ServiceResult<IdDto>> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var discount = await _repo.GetByIdAsync(request.Id);
            if (discount == null)
                return ServiceResult<IdDto>.Failed("تخفیف پیدا نشد");

            discount.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });
        }


    }
 
