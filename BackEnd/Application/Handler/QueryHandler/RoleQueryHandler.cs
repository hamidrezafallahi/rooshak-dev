using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

public class RoleQueryHandler(IRoleRepository _roleRepo,IEntityConfigRepository _configRepo) : 
        IRequestHandler<GetRolesQuery, ServiceResult<ListDto<UserRoleDto>>>,
        IRequestHandler<GetRoleByIdQuery, ServiceResult<UserRoleDto?>>
    {
        public async Task<ServiceResult<ListDto<UserRoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Role> query= _roleRepo.Query();
        
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedEntity = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        
        var rolesDto = pagedEntity.Select(x => new UserRoleDto
        {
            Id = x.Id,
            IsActive = x.IsActive,
            Name=x.RoleName
        }).ToList();
        dynamic? config = null;
        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("roles");
        }
        var resultDto = new ListDto<UserRoleDto>
        {
            Records = rolesDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        return ServiceResult<ListDto<UserRoleDto>>.Ok(resultDto);
        }
        public async Task<ServiceResult<UserRoleDto?>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleRepo.Query()
                                      .Where(r => r.Id == request.Id)
                                      .Select(r => new UserRoleDto
                                      {
                                          Id = r.Id,
                                          Name = r.RoleName
                                      })
                                      .FirstOrDefaultAsync(cancellationToken);

            if (role == null)
                return ServiceResult<UserRoleDto?>.Failed("نقش کاربری موردنظر پیدا نشد.");

            return ServiceResult<UserRoleDto?>.Ok(role);
        }

    }
