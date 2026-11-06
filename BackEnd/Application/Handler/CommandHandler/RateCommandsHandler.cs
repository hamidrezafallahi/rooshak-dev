using Application.Commands;
using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

public class RateCommandHandler(
        IRateRepository _repo,
        IHttpContextAccessor _accessor)
    : IRequestHandler<AddOrUpdateRateCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveRateCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteRateCommand, ServiceResult<IdDto>>
{

    public async Task<ServiceResult<IdDto>> Handle(AddOrUpdateRateCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        if (request.TargetId == null)
            return ServiceResult<IdDto>.Failed("مورد امتیازدهی مشخص نشده است.");
        if (request.Value == null)
            return ServiceResult<IdDto>.Failed("مقدار امتیاز ارسال نشده است.");
        if (request.TargetType == null)
            return ServiceResult<IdDto>.Failed("نوع مورد امتیازدهی مشخص نشده است.");

        var existingRate = _repo.Query(r => r.IsActive && r.TargetId == request.TargetId && r.TargetType == request.TargetType && r.UserId == userId.Value).FirstOrDefault();


        if (existingRate == null)
        {
            var rate = Rate.Create(
                userId: request.UserId ?? userId.Value,
                targetId: request.TargetId,
                targetType: request.TargetType,
                value: request.Value,
                currentUserId: userId.Value
            );

            await _repo.AddAsync(rate);
        await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = rate.Id });
        }
        else
        {
        existingRate.UpdateRate(request.Value, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = existingRate.Id });
        }


    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveRateCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var rate = await _repo.GetByIdAsync(request.Id);
        if (rate == null)
            return ServiceResult<IdDto>.Failed("امتیاز موردنظر پیدا نشد.");

        rate.SetActive(request.IsActive, userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = rate.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteRateCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var rate = await _repo.GetByIdAsync(request.Id);
        if (rate == null)
            return ServiceResult<IdDto>.Failed("امتیاز موردنظر پیدا نشد.");

        rate.Delete(userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = rate.Id });
    }

}