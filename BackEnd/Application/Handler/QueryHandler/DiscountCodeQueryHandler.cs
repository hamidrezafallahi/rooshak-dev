using Application.Common;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

public class DiscountCodeQueryHandler(IDiscountCodeRepository _repo, IEntityConfigRepository _configRepo, IHttpContextAccessor _accessor) : 
    IRequestHandler<GetDiscountCodeByCodeQuery, ServiceResult<DiscountCodeDto?>>,
    IRequestHandler<GetDiscountCodeByIdQuery, ServiceResult<DiscountCodeDto?>>,
    IRequestHandler<GetAllDiscountCodesQuery, ServiceResult<ListDto<DiscountCodeDto>>>
{
            public async Task<ServiceResult<DiscountCodeDto?>> Handle(GetDiscountCodeByCodeQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<DiscountCodeDto?>.Unauthorized();
            var now = DateTime.Now;
            var discountCode = _repo.Query(DC => DC.Code  == request.Code.ToLower() ).FirstOrDefault();
            if (discountCode == null)
                return  ServiceResult <DiscountCodeDto?>.Failed("کد تخفیف پیدا نشد");

            if (!(discountCode.StartDate <= now && discountCode.EndDate >= now))
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف اعتبار زمانی ندارد");

            if (!discountCode.IsActive)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف فعال نیست");

            if (!discountCode.IsValid)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف معتبر نیست");

            if (discountCode.UsedCount >= discountCode.UsageLimit)
                return ServiceResult<DiscountCodeDto?>.Failed("تعداد دفعات استفاده از کد تخفیف به اتمام رسیده");

            if (discountCode.UserId is not null && discountCode.UserId != userId.Value)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف متعلق به این کاربر نمی‌باشد");

            var dto = new DiscountCodeDto
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                Amount = discountCode.Amount,
                IsPercent = discountCode.IsPercent,
                StartDate = discountCode.StartDate,
                EndDate = discountCode.EndDate,
                UserId = discountCode.UserId,
                UsageLimit = discountCode.UsageLimit,
                UsedCount = discountCode.UsedCount,
                IsValid = discountCode.IsValid
            };

            return ServiceResult<DiscountCodeDto>.Ok( dto );
        }
            public async Task<ServiceResult<DiscountCodeDto?>> Handle(GetDiscountCodeByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
            return ServiceResult<DiscountCodeDto?>.Unauthorized();
            var now = DateTime.Now;
            var discountCode = _repo.Query(DC => DC.Id == request.Id).FirstOrDefault();
            if (discountCode == null)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف پیدا نشد");

            if (!(discountCode.StartDate <= now && discountCode.EndDate >= now))
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف اعتبار زمانی ندارد");

            if (!discountCode.IsActive)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف فعال نیست");

            if (!discountCode.IsValid)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف معتبر نیست");

            if (discountCode.UsedCount >= discountCode.UsageLimit)
                return ServiceResult<DiscountCodeDto?>.Failed("تعداد دفعات استفاده از کد تخفیف به اتمام رسیده");

            if (discountCode.UserId is not null && discountCode.UserId != userId.Value)
                return ServiceResult<DiscountCodeDto?>.Failed("کد تخفیف متعلق به این کاربر نمی‌باشد");

            var dto = new DiscountCodeDto
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                Amount = discountCode.Amount,
                IsPercent = discountCode.IsPercent,
                StartDate = discountCode.StartDate,
                EndDate = discountCode.EndDate,
                UserId = discountCode.UserId,
                UsageLimit = discountCode.UsageLimit,
                UsedCount = discountCode.UsedCount,
                IsValid = discountCode.IsValid
            };

            return ServiceResult<DiscountCodeDto>.Ok(dto);
        }
            public async Task<ServiceResult<ListDto<DiscountCodeDto>>> Handle(GetAllDiscountCodesQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            var now = DateTime.Now;
        IQueryable<DiscountCode> query;

            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(x => x.IsActive &&  x.StartDate<now && x.EndDate>now);
            }
       
       
 

            int totalCount = await query.CountAsync(cancellationToken);
            var pagedDiscountCode = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
 
            var dtos = query.Select(discount => new DiscountCodeDto
            {
                Id = discount.Id,
                IsActive=discount.IsActive,
                Code = discount.Code,
                Amount = discount.Amount,
                IsPercent = discount.IsPercent,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                UserId = discount.UserId,
                UsageLimit = discount.UsageLimit,
                UsedCount = discount.UsedCount,
                IsValid = discount.IsValid
            }).ToList();


            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("discountCodes");
            }
            var resultDto = new ListDto<DiscountCodeDto>
            {
                Records = dtos,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };


            return ServiceResult<ListDto<DiscountCodeDto>>.Ok(resultDto);
        }

}

