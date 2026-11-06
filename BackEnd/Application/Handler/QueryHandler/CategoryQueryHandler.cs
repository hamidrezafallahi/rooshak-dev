using Application.Dtos;
using Application.Queries;
using MediatR;
using OnlineShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Common;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

 
    public class GetCategorieQueryHandler(ICategoryRepository _repo, IEntityConfigRepository _configRepo) : 
        IRequestHandler<GetAllCategoriesQuery, ServiceResult<ListDto<CategoryDto>>>,
        IRequestHandler<GetCategoryByIdQuery, ServiceResult<CategoryDto>>,
        IRequestHandler<GetAllCategoriesIdQuery, ServiceResult<IEnumerable<IdDto>>>,
        IRequestHandler<GetAllCategoriesSlugsQuery, ServiceResult<IEnumerable<SlugDto>>>
    {
                public async Task<ServiceResult<ListDto<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            IQueryable<Category> query;
            if (request.IsShowInLanding.HasValue && request.IsShowInLanding == true) {
                query = _repo.Query(c => c.IsShowInLanding);
            } else if (request.IsShowInLanding.HasValue && request.IsShowInLanding == false) 
            {
                query = _repo.Query(c => !c.IsShowInLanding);
            } else if (request.Q is not null && request.Q.Length > 0)
            {
                query = _repo.Query(b => b.IsActive && (b.CategoryEnglishDesc.Contains(request.Q) || b.CategoryPersianDesc.Contains(request.Q) || b.EnglishName.Contains(request.Q) || b.PersianName.Contains(request.Q)));
            }
            else if(request.OnlyActives.HasValue && request.OnlyActives == false)
            {
                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(b => b.IsActive);
            }
            
            


            int totalCount = await query.CountAsync(cancellationToken);
            var pagedCategories = await query
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(cancellationToken);

            var flatDtos = pagedCategories.Select(c => new CategoryDto
            {
                Id = c.Id,
                PersianName=c.PersianName,
                CategoryPersianDesc=c.CategoryPersianDesc,
                EnglishName=c.EnglishName,
                CategoryEnglishDesc=c.CategoryEnglishDesc,
                Slug = c.Slug,
                CategoryCover = c.ImageUrl,
                IsShowInLanding=c.IsShowInLanding,
                IsActive=c.IsActive,
                ParentCategoryId = c.ParentCategoryId,
                SeoTitleFa = c.SeoTitleFa,
                SeoTitleEn = c.SeoTitleEn,
                MetaDescriptionFa = c.MetaDescriptionFa,
                MetaDescriptionEn = c.MetaDescriptionEn,
                FaqJson = c.FaqJson,
            }).ToList();


            var categoryTree = request.IsShowInLanding.HasValue ? flatDtos: CategoryTreeBuilder.BuildTree(flatDtos);

            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("categories");
            }

            var resultDto = new ListDto<CategoryDto>
            {
                Records = categoryTree,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<CategoryDto>>.Ok(resultDto);
        }
                public async Task<ServiceResult<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var key = (request.IdOrSlug ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key))
                return ServiceResult<CategoryDto>.Failed("دسته‌بندی موردنظر پیدا نشد.");

            var allCategories = await _repo.Query()
                 .Where(c => !c.IsDeleted)
                 .Select(cat => new CategoryDto
                 {
                     CategoryEnglishDesc = cat.CategoryEnglishDesc,
                     CategoryPersianDesc = cat.CategoryPersianDesc,
                     EnglishName = cat.EnglishName,
                     Id = cat.Id,
                     Slug = cat.Slug,
                     CategoryCover = cat.ImageUrl,
                     IsShowInLanding = cat.IsShowInLanding,
                     ParentCategoryId = cat.ParentCategoryId,
                     PersianName = cat.PersianName,
                     SeoTitleFa = cat.SeoTitleFa,
                     SeoTitleEn = cat.SeoTitleEn,
                     MetaDescriptionFa = cat.MetaDescriptionFa,
                     MetaDescriptionEn = cat.MetaDescriptionEn,
                     FaqJson = cat.FaqJson,
                 })

                 .ToListAsync(cancellationToken);

            CategoryDto? category = null;
            if (int.TryParse(key, out var id))
                category = allCategories.FirstOrDefault(c => c.Id == id);
            else
                category = allCategories.FirstOrDefault(c => c.Slug == key);

            if (category == null)
                return ServiceResult<CategoryDto>.Failed("دسته‌بندی موردنظر پیدا نشد.");
            category.SubCategories = CategoryTreeBuilder.BuildTree(allCategories, category.Id);
            return ServiceResult<CategoryDto>.Ok(category);
        }
                public async Task<ServiceResult<IEnumerable<IdDto>>> Handle(GetAllCategoriesIdQuery request,CancellationToken cancellationToken)
        {
            var categoryIds = await _repo.GetAllIds();
            var idDtos = categoryIds.Select(p => new IdDto
            {
                Id = p
            }).ToList();

            return ServiceResult<IEnumerable<IdDto>>.Ok(idDtos);
        }

        public async Task<ServiceResult<IEnumerable<SlugDto>>> Handle(GetAllCategoriesSlugsQuery request, CancellationToken cancellationToken)
        {
            var slugs = await _repo.Query(c => c.IsActive && !c.IsDeleted && !string.IsNullOrWhiteSpace(c.Slug))
                .Select(c => new SlugDto
                {
                    Id = c.Id,
                    Slug = c.Slug,
                    UpdatedAt = c.UpdatedAt ?? c.CreatedAt,
                    ImageUrls = string.IsNullOrWhiteSpace(c.ImageUrl)
                        ? new List<string>()
                        : new List<string> { c.ImageUrl.TrimStart('/') },
                })
                .ToListAsync(cancellationToken);
            return ServiceResult<IEnumerable<SlugDto>>.Ok(slugs);
        }
    }
    public class GetParent4selectOptionQueryHandler(ICategoryRepository _repo, IEntityConfigRepository _configRepo) : IRequestHandler<GetParent4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>
    {
        public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetParent4selectOptionQuery request, CancellationToken ct)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            int currentId = request.Id ?? 0;
            IQueryable<Category> query;
            var excludeIds = currentId == 0
        ? new List<int>()
        : await GetAllDescendants(currentId, ct); 

             query = _repo.Query(c => c.IsActive && !excludeIds.Contains(c.Id));
            int totalCount = await query.CountAsync(c => c.IsActive && !excludeIds.Contains(c.Id));
        var pagedCategories = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
 

        var flatDtos = pagedCategories.Select(c => new SelectOptionDto
        {
            Id = c.Id,
            PersianLabel = c.PersianName,
            EnglishLabel=c.EnglishName
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
        private async Task<List<int>> GetAllDescendants(int categoryId, CancellationToken ct)
        {
            var descendants = new HashSet<int> { categoryId };
            var queue = new Queue<int>();
            queue.Enqueue(categoryId);

            while (queue.Any())
            {
                var parentId = queue.Dequeue();
                var children = await _repo.Query(c => c.ParentCategoryId == parentId)
                    .Select(c => c.Id).ToListAsync(ct);

                foreach (var childId in children)
                    if (descendants.Add(childId))
                        queue.Enqueue(childId);
            }
            return descendants.ToList();
        }
    };

