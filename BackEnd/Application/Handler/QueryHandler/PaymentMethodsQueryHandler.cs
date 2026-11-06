using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

    public class PaymentMethodQueryHandler(IPaymentMethodRepository _repo,IEntityConfigRepository _configRepo) : 
        IRequestHandler<GetAllPaymentMethodsQuery, ServiceResult<ListDto<PaymentMethodDto>>>,
        IRequestHandler<GetPaymentMethods4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
        IRequestHandler<GetPaymentMethodByIdQuery, ServiceResult<PaymentMethodDto>>
    {
    public async Task<ServiceResult<ListDto<PaymentMethodDto>>> Handle(GetAllPaymentMethodsQuery request, CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
 
        IQueryable<PaymentMethod> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(b => b.Title.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive && (b.Title.Contains(request.Q)));
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
                query = _repo.Query(b => b.IsActive);
            }
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedEntity = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = pagedEntity.Select(x => new PaymentMethodDto
            {
                Id = x.Id,
                Title = x.Title,
                Code = x.Code,
                IsOnline = x.IsOnline,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                ConfigJson = x.ConfigJson
            }).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("paymentMethods");
        }
        var resultDto = new ListDto<PaymentMethodDto>
        {
            Records = dtos,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<PaymentMethodDto>>.Ok(resultDto);

        }
    public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetPaymentMethods4selectOptionQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<PaymentMethod> query;
        query = _repo.Query(c => c.IsActive);
        int totalCount = await query.CountAsync(c => c.IsActive);
        var pagedEntity = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        var flatDtos = pagedEntity.Select(c => new SelectOptionDto
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
    public async Task<ServiceResult<PaymentMethodDto>> Handle(GetPaymentMethodByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null)
                return ServiceResult<PaymentMethodDto>.Failed("روش پرداخت موردنظر پیدا نشد.");

            var dto = new PaymentMethodDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Code = entity.Code,
                IsOnline = entity.IsOnline,
                IsActive = entity.IsActive,
                DisplayOrder = entity.DisplayOrder,
                ConfigJson = entity.ConfigJson
            };

            return ServiceResult<PaymentMethodDto>.Ok(dto);
        }

    }

