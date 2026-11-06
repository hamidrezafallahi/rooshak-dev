using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

    public class ShippingMethodQueryHandler(IShippingMethodRepository _repo, IEntityConfigRepository _configRepo, IHttpContextAccessor _accessor) : 
    IRequestHandler<GetAllShippingMethodsQuery, ServiceResult<ListDto<ShippingMethodDto>>>,
    IRequestHandler<GetShippingMethodByIdQuery, ServiceResult<ShippingMethodDto>>
{
        public async Task<ServiceResult<ListDto<ShippingMethodDto>>> Handle(GetAllShippingMethodsQuery request, CancellationToken cancellationToken)
        {


            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
        IQueryable<ShippingMethod> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(b => b.Description.Contains(request.Q) || b.Title.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive && (b.Description.Contains(request.Q)|| b.Title.Contains(request.Q)));
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
                .Select(x => new ShippingMethodDto
                {
                    Id = x.Id,
                    IsActive=x.IsActive,
                    Title = x.Title,
                    Description = x.Description,
                    Price = x.Price,
                    EstimatedDeliveryTime = x.EstimatedDeliveryTime,
                    IsDefault = x.IsDefault
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        dynamic? config = null;
        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("shippingMethods");
        }
        var result = new ListDto<ShippingMethodDto>
        {
            Records = pagedEntity,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        return ServiceResult<ListDto<ShippingMethodDto>>.Ok(result);
       }
        public async Task<ServiceResult<ShippingMethodDto>> Handle(GetShippingMethodByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<ShippingMethodDto>.Unauthorized();
        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null)
            return ServiceResult<ShippingMethodDto>.Failed("متد ارسال پیدا نشد");

        var dto = new ShippingMethodDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Price = entity.Price,
            EstimatedDeliveryTime = entity.EstimatedDeliveryTime
        };
        return ServiceResult<ShippingMethodDto>.Ok(dto);
    }

}

 
