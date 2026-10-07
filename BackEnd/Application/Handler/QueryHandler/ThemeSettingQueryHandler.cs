using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class ThemeSettingQueryHandler(IThemeSettingRepository _repo, IEntityConfigRepository _configRepo) :
        IRequestHandler<GetAllThemeSettingsQuery, ServiceResult<ListDto<ThemeSettingDto>>>,
        IRequestHandler<GetThemeSettingByIdQuery, ServiceResult<ThemeSettingDto>>,
        IRequestHandler<GetActiveThemeSettingQuery, ServiceResult<ThemeSettingDto>>
    {
        public async Task<ServiceResult<ListDto<ThemeSettingDto>>> Handle(
            GetAllThemeSettingsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page is null or < 1 ? 1 : request.page.Value;
            int pageSize = request.pageSize is null or < 1 ? 10 : request.pageSize.Value;

            IQueryable<ThemeSetting> query = _repo.Query();
            if (request.OnlyActives == true)
                query = query.Where(t => t.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(t => t.Name.Contains(q));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderByDescending(t => t.IsActive)
                .ThenByDescending(t => t.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            EntityConfig? config = null;
            if (request.ByConfig == true)
                config = await _configRepo.GetByEntityNameAsync("themeSettings");

            return ServiceResult<ListDto<ThemeSettingDto>>.Ok(new ListDto<ThemeSettingDto>
            {
                Records = entities.Select(ToDto).ToList(),
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            });
        }

        public async Task<ServiceResult<ThemeSettingDto>> Handle(
            GetThemeSettingByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            return entity is null
                ? ServiceResult<ThemeSettingDto>.Failed("تم یافت نشد")
                : ServiceResult<ThemeSettingDto>.Ok(ToDto(entity));
        }

        public async Task<ServiceResult<ThemeSettingDto>> Handle(
            GetActiveThemeSettingQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetActiveAsync(cancellationToken);
            return entity is null
                ? ServiceResult<ThemeSettingDto>.Failed("تم فعالی یافت نشد")
                : ServiceResult<ThemeSettingDto>.Ok(ToDto(entity));
        }

        internal static ThemeSettingDto ToDto(ThemeSetting t) => new()
        {
            Id = t.Id,
            IsActive = t.IsActive,
            Name = t.Name,
            PrimaryColor = t.PrimaryColor,
            SecondaryColor = t.SecondaryColor,
            HighlightColor = t.HighlightColor,
            NeutralColor = t.NeutralColor,
            SuccessColor = t.SuccessColor,
            ErrorColor = t.ErrorColor,
            WarningColor = t.WarningColor,
            InfoColor = t.InfoColor,
            SurfaceColor = t.SurfaceColor,
            SurfaceMutedColor = t.SurfaceMutedColor,
            BorderColor = t.BorderColor,
            TextColor = t.TextColor,
            TextMutedColor = t.TextMutedColor,
        };
    }
}
