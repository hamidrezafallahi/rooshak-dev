using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

public class SeoSettingQueryHandler(ISeoSettingRepository _repo, IEntityConfigRepository _configRepo) :
    IRequestHandler<GetSeoSettingsQuery, ServiceResult<ListDto<SeoSettingDto>>>,
    IRequestHandler<GetSeoSettingByIdQuery, ServiceResult<SeoSettingDto>>,
    IRequestHandler<ResolveSeoSettingQuery, ServiceResult<SeoResolvedDto>>
{
    public async Task<ServiceResult<ListDto<SeoSettingDto>>> Handle(GetSeoSettingsQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;

        IQueryable<SeoSetting> query;
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            query = _repo.Query(x =>
                x.RoutePath.Contains(request.Q) ||
                (x.TitleFa != null && x.TitleFa.Contains(request.Q)) ||
                (x.TitleEn != null && x.TitleEn.Contains(request.Q)));
        }
        else if (request.OnlyActives == true)
        {
            query = _repo.Query(x => x.IsActive);
        }
        else
        {
            query = _repo.Query();
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.RoutePath)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = entities.Select(MapDto).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("seoSettings");
        }

        var resultDto = new ListDto<SeoSettingDto>
        {
            Records = dtos,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<SeoSettingDto>>.Ok(resultDto);
    }

    public async Task<ServiceResult<SeoSettingDto>> Handle(GetSeoSettingByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null)
        {
            return ServiceResult<SeoSettingDto>.Failed("تنظیمات سئوی موردنظر پیدا نشد.");
        }

        return ServiceResult<SeoSettingDto>.Ok(MapDto(entity));
    }

    public async Task<ServiceResult<SeoResolvedDto>> Handle(ResolveSeoSettingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.ResolveAsync(request.Path);
        if (entity == null)
        {
            return ServiceResult<SeoResolvedDto>.Ok(new SeoResolvedDto
            {
                RoutePath = NormalizePath(request.Path),
                MatchType = "exact",
                RobotsIndex = true,
                RobotsFollow = true,
            });
        }

        var isFa = (request.Locale ?? "fa").StartsWith("fa", StringComparison.OrdinalIgnoreCase);
        return ServiceResult<SeoResolvedDto>.Ok(new SeoResolvedDto
        {
            RoutePath = entity.RoutePath,
            MatchType = entity.MatchType,
            Title = isFa ? entity.TitleFa : entity.TitleEn,
            Description = isFa ? entity.DescriptionFa : entity.DescriptionEn,
            Keywords = isFa ? entity.KeywordsFa : entity.KeywordsEn,
            CanonicalPath = entity.CanonicalPath,
            OgImageUrl = entity.OgImageUrl,
            RobotsIndex = entity.RobotsIndex,
            RobotsFollow = entity.RobotsFollow,
        });
    }

    private static SeoSettingDto MapDto(SeoSetting entity)
    {
        return new SeoSettingDto
        {
            Id = entity.Id,
            RoutePath = entity.RoutePath,
            MatchType = entity.MatchType,
            TitleFa = entity.TitleFa,
            TitleEn = entity.TitleEn,
            DescriptionFa = entity.DescriptionFa,
            DescriptionEn = entity.DescriptionEn,
            KeywordsFa = entity.KeywordsFa,
            KeywordsEn = entity.KeywordsEn,
            CanonicalPath = entity.CanonicalPath,
            OgImageUrl = entity.OgImageUrl,
            RobotsIndex = entity.RobotsIndex,
            RobotsFollow = entity.RobotsFollow,
            Priority = entity.Priority,
            Notes = entity.Notes,
            IsActive = entity.IsActive,
        };
    }

    private static string NormalizePath(string? routePath)
    {
        return (routePath ?? string.Empty).Trim().Trim('/');
    }
}
