using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;


    public class RateQueryHandler(IRateRepository _repo,IUserRepository _userRepo, IEntityConfigRepository _configRepo)
      : IRequestHandler<GetAllRateQuery, ServiceResult<ListDto<RateDto>>>,
      IRequestHandler<GetRateByIdQuery, ServiceResult<GetRateByIdDto>>,
      IRequestHandler<GetAverageRateQuery, ServiceResult<AverageRateDto>>


{
    public async Task<ServiceResult<ListDto<RateDto>>> Handle(GetAllRateQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Rate> query;
 
        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query().Include(r=>r.User);
        }
        else
        {
            query = _repo.Query(b => b.IsActive).Include(r => r.User);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        var pagedEntity = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var dtos = pagedEntity.Select(x => new RateDto
        {
            Id = x.Id,
            IsActive=x.IsActive,
            UserName=x.User.FullName,
            RateValue=x.Value.Value,
            TargetId=x.TargetId,
            TargetType=x.TargetType.TargetTypeToDisplay()

        }).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("rates");
        }
        var resultDto = new ListDto<RateDto>
        {
            Records = dtos,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<RateDto>>.Ok(resultDto);
       
    }
    public async Task<ServiceResult<GetRateByIdDto?>> Handle(GetRateByIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var rate = await _repo.Query(r=> r.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
        if (rate == null) return ServiceResult<GetRateByIdDto?>.Failed("امتیاز موردنظر پیدا نشد.");

        var dto = new GetRateByIdDto
        {
            Id = rate.Id,
            Value= rate.Value.Value,
            TargetId=rate.TargetId,
            TargetType = rate.TargetType,
            UserId=rate.UserId
        };

        return ServiceResult<GetRateByIdDto?>.Ok(dto);
    }

    public async Task<ServiceResult<AverageRateDto>> Handle(GetAverageRateQuery request,CancellationToken cancellationToken)
        {
            var avg = await _repo.GetAverageRateAsync(
                request.TargetType, request.TargetId);
            var dto = new AverageRateDto
            {
                Average = avg.Average,
                Count = avg.Count,
            };
            return ServiceResult<AverageRateDto>.Ok(dto);
        }
    }

