using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System.Text.Json;
public class EntityConfigQueryHandler(IEntityConfigRepository _repo, IEntityConfigRepository _configRepo) : 
        IRequestHandler<GetEntityConfigsQuery, ServiceResult<ListDto<EntityConfigDto>>>,
        IRequestHandler<GetMenuQuery, ServiceResult<List<MenuReadModel>>>,
        IRequestHandler<GetEntityConfigByIdQuery, ServiceResult<EntityConfigDto?>>,
        IRequestHandler<GetEntityConfigByNameQuery, ServiceResult<EntityConfigDto?>>,
        IRequestHandler<GetFormQuery, ServiceResult<FormReadModel>>
    {
 
        public async Task<ServiceResult<ListDto<EntityConfigDto>>> Handle(GetEntityConfigsQuery request, CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<EntityConfig> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(b => b.EntityName.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive && (b.EntityName.Contains(request.Q)));
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
        var configsDto = pagedEntity.Select(c => new EntityConfigDto
            {
                Id = c.Id,
                EntityName = c.EntityName,
                EndPoint = c.EndPoint,
                PersianDisplayName = c.PersianDisplayName,
                EnglishDisplayName = c.EnglishDisplayName,
                ColumnsJson = c.ColumnsJson ?? "[]",// JsonSerializer.Deserialize<List<JsonDefinition>>(c.ColumnsJson ?? "[]"),// c.ColumnsJson ?? "[]",  
                ActionsJson = c.ActionsJson ?? "[]",
                EntityIconBase64 = c.EntityIconBase64,
                FormFieldsJson= c.FormFieldsJson,//JsonSerializer.Deserialize<List<FormFieldDefinition>>(c.FormFieldsJson ?? "[]"),//c.FormFieldsJson,
            IsActive =c.IsActive,
            }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("entityConfigs");
        }
        var resultDto = new ListDto<EntityConfigDto>
        {
            Records = configsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        return ServiceResult<ListDto<EntityConfigDto>>.Ok(resultDto);
        }
        public async Task<ServiceResult<List<MenuReadModel>>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
        {
            var menuList =  _repo.Query(e => e.IsActive)
                 .Select(z => new MenuReadModel
                 {
                     EntityName = z.EntityName,
                     EnglishDisplayName = z.EnglishDisplayName,
                     PersianDisplayName = z.PersianDisplayName,
                     EndPoint = z.EndPoint,
                     EntityIconBase64 = z.EntityIconBase64,
                 }).ToList();
            

            return ServiceResult<List<MenuReadModel>>.Ok(menuList);
        }
        public async Task<ServiceResult<EntityConfigDto?>> Handle(GetEntityConfigByIdQuery request, CancellationToken cancellationToken)
        {
            var config = await _repo.GetByIdAsync(request.Id);
            if (config == null)
                return ServiceResult<EntityConfigDto?>.Failed("پیکربندی موردنظر پیدا نشد.");


            var configDto = new EntityConfigDto
            {
                Id = config.Id,
                EntityName = config.EntityName,
                EnglishDisplayName = config.EnglishDisplayName,
                PersianDisplayName = config.PersianDisplayName,
                EntityIconBase64 = config.EntityIconBase64,
                EndPoint = config.EndPoint,
                ColumnsJson = config.ColumnsJson,
                ActionsJson = config.ActionsJson,
                FormFieldsJson = config.FormFieldsJson,
                IsActive = config.IsActive,

            };

            return ServiceResult<EntityConfigDto?>.Ok(configDto);
        }
        public async Task<ServiceResult<EntityConfigDto?>> Handle(GetEntityConfigByNameQuery request, CancellationToken cancellationToken)
        {
            var config = await _repo.GetByEntityNameAsync(request.EntityName);
            if (config == null)
                return ServiceResult<EntityConfigDto?>.Failed("پیکربندی موردنظر پیدا نشد.");

            var configDto = new EntityConfigDto
            {
                Id = config.Id,
                EntityName = config.EntityName,
                EnglishDisplayName = config.EnglishDisplayName,
                PersianDisplayName = config.PersianDisplayName,
                EntityIconBase64 = config.EntityIconBase64,
                EndPoint = config.EndPoint,
                ColumnsJson = config.ColumnsJson,
                ActionsJson = config.ActionsJson,
                IsActive=config.IsActive,
                
            };


            return ServiceResult<EntityConfigDto?>.Ok(configDto);
        }
        public async Task<ServiceResult<FormReadModel?>> Handle(GetFormQuery request, CancellationToken cancellationToken)
        {
            var config = await _repo.GetByEntityNameAsync(request.EntityName);
            if (config == null)
                return ServiceResult<FormReadModel?>.Failed("پیکربندی فرم موردنظر پیدا نشد.");

            var configDto = new FormReadModel
            {
                EntityName = config.EntityName,
                EnglishDisplayName = config.EnglishDisplayName,
                PersianDisplayName = config.PersianDisplayName,
                EntityIconBase64 = config.EntityIconBase64,
                EndPoint = config.EndPoint,
                FormFieldsJson = config.FormFieldsJson
            };


            return ServiceResult<FormReadModel?>.Ok(configDto);
        }

    }

 
