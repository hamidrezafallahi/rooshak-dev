using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class BlogQueryHandler(
            IBlogRepository _repo,
             IEntityConfigRepository _configRepo) :
        IRequestHandler<GetBlogsQuery, ServiceResult<ListDto<BlogDto>>>,
        IRequestHandler<GetBlogs4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
        IRequestHandler<GetBlogByIdQuery, ServiceResult<BlogDto?>>,
        IRequestHandler<GetBlogBySlugQuery, ServiceResult<BlogDto?>>,
        IRequestHandler<GetAllBlogsSlugsQuery, ServiceResult<IEnumerable<SlugDto>>>
{



    public async Task<ServiceResult<ListDto<BlogDto>>> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
 
        IQueryable<Blog> query;
        var includeInactive = request.ByConfig == true
            || !request.OnlyActives.HasValue
            || request.OnlyActives == false;

        if (request.Q is not null && request.Q.Length > 0)
        {
            if (includeInactive)
            {
                query = _repo.Query(b => b.Slug.Contains(request.Q) || b.ContentFa.Contains(request.Q) ||
                                        b.ContentEn.Contains(request.Q) || b.TitleFa.Contains(request.Q) ||
                                        b.TitleEn.Contains(request.Q)).Include(x => x.Author);
            }
            else
            {
                query = _repo.Query(b => b.IsActive && (b.Slug.Contains(request.Q) || b.ContentFa.Contains(request.Q) ||
                      b.ContentEn.Contains(request.Q) || b.TitleFa.Contains(request.Q) ||
                                                      b.TitleEn.Contains(request.Q))).Include(x => x.Author);
            }
        }
        else
        {
            if (includeInactive)
            {
                query = _repo.Query().Include(x=>x.Author);
            }
            else
            {
                query = _repo.Query(b => b.IsActive).Include(x => x.Author);
            }
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedBlogs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var blogsDto = pagedBlogs.Select(x => new BlogDto
        {
            Id = x.Id,
            TitleFa = x.TitleFa,
            IntroFa = x.IntroFa,
            ContentFa = x.ContentFa,
            ConclusionFa = x.ConclusionFa,
            ExcerptFa = x.ExcerptFa,
            MetaDescriptionFa = x.MetaDescriptionFa,
            MetaKeywordsFa = x.MetaKeywordsFa,

            TitleEn = x.TitleEn,
            IntroEn = x.IntroEn,
            ContentEn = x.ContentEn,
            ConclusionEn= x.ConclusionEn,
            ExcerptEn = x.ExcerptEn,
            MetaDescriptionEn = x.MetaDescriptionEn,
            MetaKeywordsEn = x.MetaKeywordsEn,

            Slug = x.Slug,
            AuthorName = x.Author.FullName,
            ThumbnailFile = x.ThumbnailFile,
            IsActive = x.IsActive
        }).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("blogs");
        }
        var resultDto = new ListDto<BlogDto>
        {
            Records = blogsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<BlogDto>>.Ok(resultDto);
        //var gridifyQuery = new GridifyQuery
        //{
        //    Page = request.page ?? 1,
        //    PageSize = request.pageSize ?? 10,
        //    Filter = BuildFilterExpression(request),
        //    Sort = request.SortBy ?? "CreatedAt"
        //};

        //var query = _repo.Query()
        //   .Include(x => x.Author)
        //   .AsQueryable();

        //query = ApplyRoleBasedFilter(query, request.UserRole);

        //var result = await query.GridifyAsync(gridifyQuery, cancellationToken);

        //var req = _accessor.HttpContext?.Request;
        //string domainUrl = req != null ? $"{req.Scheme}://{req.Host}" : "";

        //var blogsDto = result.Data.Select(x => new BlogDto
        //{
        //    Id = x.Id,
        //    TitleFa = x.TitleFa,
        //    IntroFa = x.IntroFa,
        //    ContentFa = x.ContentFa,
        //    ConclusionFa = x.ConclusionFa,
        //    ExcerptFa = x.ExcerptFa,
        //    MetaDescriptionFa = x.MetaDescriptionFa,
        //    MetaKeywordsFa = x.MetaKeywordsFa,

        //    TitleEn = x.TitleEn,
        //    IntroEn = x.IntroEn,
        //    ContentEn = x.ContentEn,
        //    ConclusionEn = x.ConclusionEn,
        //    ExcerptEn = x.ExcerptEn,
        //    MetaDescriptionEn = x.MetaDescriptionEn,
        //    MetaKeywordsEn = x.MetaKeywordsEn,

        //    Slug = x.Slug,
        //    AuthorName = x.Author.FullName,
        //    ThumbnailFile = !string.IsNullOrEmpty(x.ThumbnailFile)
        //        ? $"{domainUrl}/{x.ThumbnailFile.TrimStart('/')}"
        //        : null,
        //    IsActive = x.IsActive
        //}).ToList();
        //dynamic? config = null;

        //if (request.ByConfig == true)
        //{
        //    config = await _configRepo.GetByEntityNameAsync("blogs");
        //}
        //var resultDto = new ListDto<BlogDto>
        //{
        //    Records = blogsDto,
        //    ColumnsJson = config?.ColumnsJson,
        //    ActionsJson = config?.ActionsJson,
        //    TotalCount = result.Count,
        //    PageNumber = gridifyQuery.Page,
        //    PageSize = gridifyQuery.PageSize,
        //};

        //return ServiceResult<ListDto<BlogDto>>.Ok(resultDto);
    }
    public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetBlogs4selectOptionQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Blog> query;
        query = _repo.Query(c => c.IsActive);
        int totalCount = await query.CountAsync(c => c.IsActive);
        var pagedEntity = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();
        var flatDtos = pagedEntity.Select(x => new SelectOptionDto
        {
            Id = x.Id,
            PersianLabel = x.TitleFa,
            EnglishLabel = x.TitleEn
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
    public async Task<ServiceResult<BlogDto?>> Handle(GetBlogByIdQuery request, CancellationToken cancellationToken)
    {
        var x =  _repo.Query(b => b.Id == request.Id).Include(x => x.Author)
                   .FirstOrDefault();

        if (x == null)
            return ServiceResult<BlogDto?>.Failed("Blog not found");
        var blogDto = new BlogDto
        {
            Id = x.Id,
            TitleFa = x.TitleFa,
            IntroFa = x.IntroFa,
            ContentFa = x.ContentFa,
            ConclusionFa = x.ConclusionFa,
            ExcerptFa = x.ExcerptFa,
            MetaDescriptionFa = x.MetaDescriptionFa,
            MetaKeywordsFa = x.MetaKeywordsFa,

            TitleEn = x.TitleEn,
            IntroEn = x.IntroEn,
            ContentEn = x.ContentEn,
            ConclusionEn = x.ConclusionEn,
            ExcerptEn = x.ExcerptEn,
            MetaDescriptionEn = x.MetaDescriptionEn,
            MetaKeywordsEn = x.MetaKeywordsEn,

            Slug = x.Slug,
            AuthorName = x.Author.FullName,
            ThumbnailFile = x.ThumbnailFile,
            IsActive = x.IsActive
        };
        return ServiceResult<BlogDto?>.Ok(blogDto);
    }
    public async Task<ServiceResult<BlogDto?>> Handle(GetBlogBySlugQuery request, CancellationToken cancellationToken)
    {
        var x = await _repo.Query(x => x.Slug == request.Slug.Trim() && x.IsActive).Include(x => x.Author).Include(x => x.BlogTags).ThenInclude(x => x.Tag)
                   .FirstOrDefaultAsync();

        if (x == null)
            return ServiceResult<BlogDto?>.Failed("Blog not found");
        var blogDto = new BlogDto
        {
            Id = x.Id,
            TitleFa = x.TitleFa,
            IntroFa = x.IntroFa,
            ContentFa = x.ContentFa,
            ConclusionFa = x.ConclusionFa,
            ExcerptFa = x.ExcerptFa,
            MetaDescriptionFa = x.MetaDescriptionFa,
            MetaKeywordsFa = x.MetaKeywordsFa,

            TitleEn = x.TitleEn,
            IntroEn = x.IntroEn,
            ContentEn = x.ContentEn,
            ConclusionEn = x.ConclusionEn,
            ExcerptEn = x.ExcerptEn,
            MetaDescriptionEn = x.MetaDescriptionEn,
            MetaKeywordsEn = x.MetaKeywordsEn,

            Slug = x.Slug,
            AuthorName = x.Author.FullName,
            ThumbnailFile = x.ThumbnailFile,
            BlogTags = x.BlogTags.Where(bt=>!bt.IsDeleted).Select(t=>new TagDto
            {
                Id=t.Tag.Id,
                Name=t.Tag.Name,
                IsActive=t.Tag.IsActive
            }).ToList(),
            IsActive = x.IsActive,
            CreatedAt=x.CreatedAt,
            UpdatedAt=x.UpdatedAt
        };
        return ServiceResult<BlogDto?>.Ok(blogDto);
    }
    public async Task<ServiceResult<IEnumerable<SlugDto>>> Handle(GetAllBlogsSlugsQuery request, CancellationToken cancellationToken)
    {
        var query = request.IncludeInactive
            ? _repo.Query(b => !b.IsDeleted)
            : _repo.Query(b => b.IsActive && !b.IsDeleted);

        var blogsSlugs = await query
         .Select(p => new SlugDto
         {
             Id = p.Id,
             Slug = p.Slug,
             UpdatedAt = p.UpdatedAt ?? p.CreatedAt,
             ImageUrls = string.IsNullOrWhiteSpace(p.ThumbnailFile)
                ? new List<string>()
                : new List<string> { p.ThumbnailFile.TrimStart('/') },
         }).ToListAsync(cancellationToken);

        return ServiceResult<IEnumerable<SlugDto>>.Ok(blogsSlugs);
    }


    //private string BuildFilterExpression(GetBlogsQuery request)
    //{
    //    var filters = new List<string>();

    //    // فیلتر فعال بودن
    //    if (request.OnlyActives == true)
    //    {
    //        filters.Add("isActive=true");
    //    }

    //    // فیلتر جستجو (Q)
    //    if (!string.IsNullOrEmpty(request.Q))
    //    {
    //        // جستجو در چندین فیلد همزمان با OR
    //        var searchFilter = $"(titleFa*={request.Q}|titleEn*={request.Q}|contentFa*={request.Q}|contentEn*={request.Q}|slug*={request.Q})";
    //        filters.Add(searchFilter);
    //    }

    //    // ترکیب همه فیلترها با AND
    //    return filters.Any() ? string.Join(",", filters) : "";
    //}

    //private IQueryable<Blog> ApplyRoleBasedFilter(IQueryable<Blog> query, string userRole)
    //{
    //    if (userRole == "Admin")
    //        return query; // ادمین همه چیز را می‌بیند

    //    // مشتری فقط مطالب فعال و منتشر شده را می‌بیند
    //    return query.Where(b => b.IsActive && b.CreatedAt <= DateTime.UtcNow);
    //}
}

