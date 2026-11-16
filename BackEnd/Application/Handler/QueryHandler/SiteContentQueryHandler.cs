using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class SiteContentQueryHandler(
        IHomeSlideRepository _slides,
        IAboutSectionRepository _sections,
        ICustomFontRepository _fonts,
        IEntityConfigRepository _configRepo) :
        IRequestHandler<GetAllHomeSlidesQuery, ServiceResult<ListDto<HomeSlideDto>>>,
        IRequestHandler<GetHomeSlideByIdQuery, ServiceResult<HomeSlideDto>>,
        IRequestHandler<GetPublicHomeSlidesQuery, ServiceResult<List<HomeSlideDto>>>,
        IRequestHandler<GetAllAboutSectionsQuery, ServiceResult<ListDto<AboutSectionDto>>>,
        IRequestHandler<GetAboutSectionByIdQuery, ServiceResult<AboutSectionDto>>,
        IRequestHandler<GetPublicAboutSectionsQuery, ServiceResult<List<AboutSectionDto>>>,
        IRequestHandler<GetAllCustomFontsQuery, ServiceResult<ListDto<CustomFontDto>>>,
        IRequestHandler<GetCustomFontByIdQuery, ServiceResult<CustomFontDto>>,
        IRequestHandler<GetPublicCustomFontsQuery, ServiceResult<List<CustomFontDto>>>
    {
        // ===== اسلایدر میانی =====
        public async Task<ServiceResult<ListDto<HomeSlideDto>>> Handle(GetAllHomeSlidesQuery request, CancellationToken ct)
        {
            var query = _slides.Query();
            if (request.OnlyActives == true) query = query.Where(s => s.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(s => s.TitleFa.Contains(q) || s.TitleEn.Contains(q) || s.LinkUrl.Contains(q));
            }

            return await Page(request, "homeSlides",
                query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id), ToDto, ct);
        }

        public async Task<ServiceResult<HomeSlideDto>> Handle(GetHomeSlideByIdQuery request, CancellationToken ct)
        {
            var entity = await _slides.GetByIdAsync(request.Id);
            return entity is null
                ? ServiceResult<HomeSlideDto>.Failed("اسلاید پیدا نشد")
                : ServiceResult<HomeSlideDto>.Ok(ToDto(entity));
        }

        public async Task<ServiceResult<List<HomeSlideDto>>> Handle(GetPublicHomeSlidesQuery request, CancellationToken ct)
        {
            var list = await _slides.Query(s => s.IsActive && s.ImageUrl != "")
                .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id)
                .AsNoTracking().ToListAsync(ct);
            return ServiceResult<List<HomeSlideDto>>.Ok(list.Select(ToDto).ToList());
        }

        // ===== درباره ما =====
        public async Task<ServiceResult<ListDto<AboutSectionDto>>> Handle(GetAllAboutSectionsQuery request, CancellationToken ct)
        {
            var query = _sections.Query();
            if (request.OnlyActives == true) query = query.Where(s => s.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(s => s.SectionKey.Contains(q) || s.TitleFa.Contains(q) || s.BodyFa.Contains(q));
            }

            return await Page(request, "aboutSections",
                query.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id), ToDto, ct);
        }

        public async Task<ServiceResult<AboutSectionDto>> Handle(GetAboutSectionByIdQuery request, CancellationToken ct)
        {
            var entity = await _sections.GetByIdAsync(request.Id);
            return entity is null
                ? ServiceResult<AboutSectionDto>.Failed("بخش پیدا نشد")
                : ServiceResult<AboutSectionDto>.Ok(ToDto(entity));
        }

        public async Task<ServiceResult<List<AboutSectionDto>>> Handle(GetPublicAboutSectionsQuery request, CancellationToken ct)
        {
            var list = await _sections.Query(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder).ThenBy(s => s.Id)
                .AsNoTracking().ToListAsync(ct);
            return ServiceResult<List<AboutSectionDto>>.Ok(list.Select(ToDto).ToList());
        }

        // ===== فونت‌ها =====
        public async Task<ServiceResult<ListDto<CustomFontDto>>> Handle(GetAllCustomFontsQuery request, CancellationToken ct)
        {
            var query = _fonts.Query();
            if (request.OnlyActives == true) query = query.Where(f => f.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(f => f.NameFa.Contains(q) || f.NameEn.Contains(q));
            }

            return await Page(request, "customFonts", query.OrderBy(f => f.Id), ToDto, ct);
        }

        public async Task<ServiceResult<CustomFontDto>> Handle(GetCustomFontByIdQuery request, CancellationToken ct)
        {
            var entity = await _fonts.GetByIdAsync(request.Id);
            return entity is null
                ? ServiceResult<CustomFontDto>.Failed("فونت پیدا نشد")
                : ServiceResult<CustomFontDto>.Ok(ToDto(entity));
        }

        public async Task<ServiceResult<List<CustomFontDto>>> Handle(GetPublicCustomFontsQuery request, CancellationToken ct)
        {
            var list = await _fonts.Query(f => f.IsActive && f.FileUrl != "")
                .OrderBy(f => f.Id).AsNoTracking().ToListAsync(ct);
            return ServiceResult<List<CustomFontDto>>.Ok(list.Select(ToDto).ToList());
        }

        // ===== کمکی =====
        private async Task<ServiceResult<ListDto<TDto>>> Page<TEntity, TDto>(
            BaseListDto request, string entityName, IQueryable<TEntity> ordered,
            Func<TEntity, TDto> map, CancellationToken ct)
        {
            int pageNumber = request.page is null or < 1 ? 1 : request.page.Value;
            int pageSize = request.pageSize is null or < 1 ? 10 : request.pageSize.Value;

            var total = await ordered.CountAsync(ct);
            var items = await ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            EntityConfig? config = request.ByConfig == true
                ? await _configRepo.GetByEntityNameAsync(entityName)
                : null;

            return ServiceResult<ListDto<TDto>>.Ok(new ListDto<TDto>
            {
                Records = items.Select(map).ToList(),
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
            });
        }

        internal static HomeSlideDto ToDto(HomeSlide s) => new()
        {
            Id = s.Id,
            IsActive = s.IsActive,
            ImageUrl = s.ImageUrl,
            MobileImageUrl = s.MobileImageUrl,
            LinkUrl = s.LinkUrl,
            TitleFa = s.TitleFa,
            TitleEn = s.TitleEn,
            DisplayOrder = s.DisplayOrder,
        };

        internal static AboutSectionDto ToDto(AboutSection s) => new()
        {
            Id = s.Id,
            IsActive = s.IsActive,
            SectionKey = s.SectionKey,
            DisplayOrder = s.DisplayOrder,
            EyebrowFa = s.EyebrowFa,
            EyebrowEn = s.EyebrowEn,
            TitleFa = s.TitleFa,
            TitleEn = s.TitleEn,
            BodyFa = s.BodyFa,
            BodyEn = s.BodyEn,
            LinkTextFa = s.LinkTextFa,
            LinkTextEn = s.LinkTextEn,
            LinkUrl = s.LinkUrl,
        };

        internal static CustomFontDto ToDto(CustomFont f) => new()
        {
            Id = f.Id,
            IsActive = f.IsActive,
            Key = f.Key,
            NameFa = f.NameFa,
            NameEn = f.NameEn,
            Category = f.Category,
            FileUrl = f.FileUrl,
        };
    }
}
