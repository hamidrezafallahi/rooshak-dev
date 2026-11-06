using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class BlogTagQueryHandler(IBlogTagRepository _repo,
             IEntityConfigRepository _configRepo)
        : IRequestHandler<GetAllBlogTagsQuery, ServiceResult<ListDto<BlogTagDto>>>,
        IRequestHandler<GetBlogTagByIdQuery, ServiceResult<BlogTagDto>>
 
{
       public async Task<ServiceResult<ListDto<BlogTagDto>>> Handle(GetAllBlogTagsQuery request,CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
        IQueryable<BlogTag> query;
        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query().Include(x => x.Tag).Include(x => x.Blog);
               
        }
        else
        {
            query = _repo.Query(b => b.IsActive).Include(x => x.Tag).Include(x=>x.Blog);
        }
            int totalCount = await query.CountAsync(cancellationToken);
            var pagedEntity = await query.Where(pt => !pt.IsDeleted)
                .Include(pt => pt.Tag)
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);
            var ProductTagsDto = pagedEntity.Select(x => new BlogTagDto
            {
                Id = x.Id,
                IsActive=x.IsActive,
                BlogId=x.BlogId,
                BlogTitleFa=x.Blog.TitleFa,
                BlogTitleEn = x.Blog.TitleEn,
                TagName = x.Tag?.Name ?? string.Empty
            }).ToList();
            dynamic? config = null;
            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("BlogTags");
            }
            var resultDto = new ListDto<BlogTagDto>
            {
                Records = ProductTagsDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            return ServiceResult<ListDto<BlogTagDto>>.Ok(resultDto);
        }
       public async Task<ServiceResult<BlogTagDto>> Handle(GetBlogTagByIdQuery request, CancellationToken cancellationToken)
        {
            var BlogTag = await _repo
                .Query()
                .Include(pot => pot.Tag)
                .FirstOrDefaultAsync(pot => pot.Id == request.Id, cancellationToken);

            if (BlogTag == null)
                return ServiceResult<BlogTagDto>.Failed("برچسب مقالهٔ موردنظر پیدا نشد.");
            var dto = new BlogTagDto
            {
                Id = BlogTag.Id,
                IsActive = BlogTag.IsActive,
                BlogId = BlogTag.BlogId,
                TagId = BlogTag.TagId,
             };

            return ServiceResult<BlogTagDto>.Ok(dto);
        }

}

