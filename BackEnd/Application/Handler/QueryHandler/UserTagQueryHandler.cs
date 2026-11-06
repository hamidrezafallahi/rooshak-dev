using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class UserTagQueryHandler(IUserTagRepository _repo,
            IHttpContextAccessor _accessor,
            IEntityConfigRepository _configRepo)
        : IRequestHandler<GetAllUserTagsQuery, ServiceResult<ListDto<UserTagDto>>>,
        IRequestHandler<GetUserTagByIdQuery, ServiceResult<UserTagDto>>
 
{
       public async Task<ServiceResult<ListDto<UserTagDto>>> Handle(GetAllUserTagsQuery request,CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
        IQueryable<UserTag> query;
        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query().Include(x => x.Tag).Include(x=>x.User);
               
        }
        else
        {
            query = _repo.Query(b => b.IsActive).Include(x => x.Tag).Include(x => x.User);
        }
            int totalCount = await query.CountAsync(cancellationToken);
            var pagedEntity = await query.Where(pt => !pt.IsDeleted)
                .Include(pt => pt.Tag)
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);
            var ProductTagsDto = pagedEntity.Select(x => new UserTagDto
            {
                Id = x.Id,
                IsActive=x.IsActive,
                TagName = x.Tag.Name,
                UserName=x.User.FullName
            }).ToList();
            dynamic? config = null;
            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("UserTags");
            }
            var resultDto = new ListDto<UserTagDto>
            {
                Records = ProductTagsDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            return ServiceResult<ListDto<UserTagDto>>.Ok(resultDto);
        }
       public async Task<ServiceResult<UserTagDto>> Handle(GetUserTagByIdQuery request, CancellationToken cancellationToken)
        {
            var UserTag = await _repo
                .Query()
                .Include(pot => pot.Tag)
                .FirstOrDefaultAsync(pot => pot.Id == request.Id, cancellationToken);

            if (UserTag == null)
                return ServiceResult<UserTagDto>.Failed("برچسب کاربر موردنظر پیدا نشد.");

            

            var dto = new UserTagDto
            {
                Id = UserTag.Id,
                IsActive = UserTag.IsActive,
                UserId = UserTag.UserId,
                TagId= UserTag.TagId,
             };

            return ServiceResult<UserTagDto>.Ok(dto);
        }

}

