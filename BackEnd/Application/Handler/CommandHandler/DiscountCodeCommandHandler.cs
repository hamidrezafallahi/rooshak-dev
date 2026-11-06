using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;

public class DiscountCodeCommandHandler(IDiscountCodeRepository _repo, IHttpContextAccessor _accessor) :
    IRequestHandler<CreateDiscountCodeCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateDiscountCodeCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveDiscountCodeCommand, ServiceResult<IdDto>>,
     IRequestHandler<DeleteDiscountCodeCommand, ServiceResult<IdDto>>

{
    public async Task<ServiceResult<IdDto>> Handle(CreateDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId is null)
            return ServiceResult<IdDto>.Unauthorized();
        var discount = DiscountCode.Create(
            code: request.Code.ToLower(),
            amount: request.Amount,
            isPercent: request.IsPercent,
            startDate: request.StartDate,
            endDate: request.EndDate,
            userId: request.UserId,
            usageLimit: request.UsageLimit,
            currentUserId: userId.Value
        );

        await _repo.AddAsync(discount);
        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });

    }
    public async Task<ServiceResult<IdDto>> Handle(UpdateDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId is null)
            return ServiceResult<IdDto>.Unauthorized();
        var discount = await _repo.GetByIdAsync(request.Id);
        if (discount == null)
            return ServiceResult<IdDto>.Failed("کد تخفیف پیدا نشد");

        discount.Update(
            code: request.Code.ToLower(),
            amount: request.Amount,
            isPercent: request.IsPercent,
            startDate: request.StartDate,
            endDate: request.EndDate,
            userId: request.UserId,
            usageLimit: request.UsageLimit,
            currentUserId: request.CurrentUserId
        );

        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveDiscountCodeCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();

        var discountCode = await _repo.GetByIdAsync(request.Id);
        if (discountCode == null)
            return ServiceResult<IdDto>.Failed("کد تخفیف پیدا نشد.");
        discountCode.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = discountCode.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteDiscountCodeCommand request, CancellationToken cancellationToken)
    {


        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var discount = await _repo.GetByIdAsync(request.Id);
        if (discount == null)
            return ServiceResult<IdDto>.Failed("این کد تخفیف موجود نیست");

        discount.Delete(userId.Value);

        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = discount.Id });

    }
}

 
