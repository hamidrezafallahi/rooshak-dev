using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

public class  DiscountQueryHandler(IDiscountRepository _repo, IEntityConfigRepository _configRepo) : 
    IRequestHandler<GetAllDiscountsQuery, ServiceResult<ListDto<DiscountDto>>>,
    IRequestHandler<GetDiscountByIdQuery, ServiceResult<DiscountDto>>,
    IRequestHandler<GetDiscounts4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
    IRequestHandler<GetActiveDiscountsQuery, ServiceResult<IEnumerable<DiscountDto>>>,
    IRequestHandler<GetAllDiscountsIdQuery, ServiceResult<IEnumerable<IdDto>>>,
    IRequestHandler<GetAllDiscountsSlugsQuery, ServiceResult<IEnumerable<SlugDto>>>,
    IRequestHandler<GetDiscountByProductOfferIdQuery, ServiceResult<Discount?>>,
    IRequestHandler<IsDiscountValidQuery, ServiceResult<ValidDiscountDto?>>
{
    public async Task<ServiceResult<ListDto<DiscountDto>>> Handle(GetAllDiscountsQuery request, CancellationToken cancellationToken)
        {
        var now = DateTime.Now;
        int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            IQueryable<Discount> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(x => x.Title.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(x => (x.IsActive  &&
                  x.StartDate <= now && x.EndDate >= now) && (x.Title.Contains(request.Q)));
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(x => x.IsActive  &&
                  x.StartDate <= now && x.EndDate >= now);
            }
        }
       
            int totalCount = await query.CountAsync(cancellationToken);
            var pagedDiscounts = await query
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);
            var dtoList = pagedDiscounts.Select(d => new DiscountDto
            {
                Id = d.Id,
                IsActive = d.IsActive,
                Amount = d.Amount,
                IsPercent = d.IsPercent,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Title = d.Title,
                
            }).ToList()   ;

            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("discounts");
            }


            var resultDto = new ListDto<DiscountDto>
            {
                Records = dtoList,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            return ServiceResult<ListDto<DiscountDto>>.Ok(resultDto);

         }
    public async Task<ServiceResult<DiscountDto>> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken)
        {
            var discount = await _repo.GetByIdAsync(request.Id);
            var discountDto = new DiscountDto
            {
                Id = discount.Id,
                IsActive = discount.IsActive,
                Amount = discount.Amount,
                IsPercent = discount.IsPercent,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate,
                Title = discount.Title,
            };


            return ServiceResult<DiscountDto>.Ok(discountDto);
        }
    public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetDiscounts4selectOptionQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        var now = DateTime.UtcNow;
        IQueryable<Discount> query;
        query = _repo.Query(c => c.IsActive   &&
                  c.StartDate <= now && c.EndDate >= now);
        int totalCount = await query.CountAsync(c => c.IsActive);
        var pagedDiscounts = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        var flatDtos = pagedDiscounts.Select(c => new SelectOptionDto
        {
            Id = c.Id,
            PersianLabel = c.Title,
            EnglishLabel = c.Title
        }).ToList();
        var resultDto = new ListDto<SelectOptionDto>
        {
            Records = flatDtos,
            ColumnsJson = null,
            ActionsJson = null,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<SelectOptionDto>>.Ok(resultDto);
    }
    public async Task<ServiceResult<IEnumerable<DiscountDto>>> Handle(GetActiveDiscountsQuery request, CancellationToken cancellationToken)
        {
            var discounts = await _repo.GetActiveDiscountsAsync();
            var dtoList = discounts.Select(d => new DiscountDto
            {
                Id = d.Id,
                IsActive = d.IsActive,
                Amount = d.Amount,
                IsPercent = d.IsPercent,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                Title = d.Title,
            }).ToList();


            return ServiceResult<IEnumerable<DiscountDto>>.Ok(dtoList);
        }

    public async Task<ServiceResult<IEnumerable<IdDto>>> Handle(GetAllDiscountsIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var ids = await _repo.Query(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartDate <= now &&
                d.EndDate >= now)
            .Select(d => new IdDto { Id = d.Id })
            .ToListAsync(cancellationToken);

        return ServiceResult<IEnumerable<IdDto>>.Ok(ids);
    }

    public async Task<ServiceResult<IEnumerable<SlugDto>>> Handle(GetAllDiscountsSlugsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var slugs = await _repo.Query(d =>
                !d.IsDeleted &&
                d.IsActive &&
                d.StartDate <= now &&
                d.EndDate >= now)
            .Select(d => new SlugDto
            {
                Id = d.Id,
                // Route key for /discounts/[slug] is the numeric id.
                Slug = d.Id.ToString(),
                UpdatedAt = d.UpdatedAt ?? d.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IEnumerable<SlugDto>>.Ok(slugs);
    }
    public async Task<ServiceResult<Discount?>> Handle(GetDiscountByProductOfferIdQuery request, CancellationToken cancellationToken)
        {
            var discount = await _repo.GetDiscountByProductOfferIdAsync(request.ProductId);

            if (discount == null)
                return ServiceResult<Discount?>.Failed("Discount not found");

            return ServiceResult<Discount?>.Ok(discount);
        }
    public async Task<ServiceResult<ValidDiscountDto?>> Handle(IsDiscountValidQuery request, CancellationToken cancellationToken)
        {
            var isValid = await _repo.IsDiscountValidAsync(request.DiscountId);

            return ServiceResult<ValidDiscountDto?>.Ok(new ValidDiscountDto
            {
                Valid = isValid
            });
        }
}

