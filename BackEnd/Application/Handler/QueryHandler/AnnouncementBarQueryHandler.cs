using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class AnnouncementBarQueryHandler(IAnnouncementBarRepository _repo, IEntityConfigRepository _configRepo) :
        IRequestHandler<GetAllAnnouncementBarsQuery, ServiceResult<ListDto<AnnouncementBarDto>>>,
        IRequestHandler<GetAnnouncementBarByIdQuery, ServiceResult<AnnouncementBarDto>>,
        IRequestHandler<GetCurrentAnnouncementBarQuery, ServiceResult<AnnouncementBarDto>>
    {
        public async Task<ServiceResult<ListDto<AnnouncementBarDto>>> Handle(
            GetAllAnnouncementBarsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page is null or < 1 ? 1 : request.page.Value;
            int pageSize = request.pageSize is null or < 1 ? 10 : request.pageSize.Value;

            IQueryable<AnnouncementBar> query = _repo.Query();
            if (request.OnlyActives == true)
                query = query.Where(b => b.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(b => b.MessageFa.Contains(q) || b.MessageEn.Contains(q));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            EntityConfig? config = null;
            if (request.ByConfig == true)
                config = await _configRepo.GetByEntityNameAsync("announcementBars");

            return ServiceResult<ListDto<AnnouncementBarDto>>.Ok(new ListDto<AnnouncementBarDto>
            {
                Records = entities.Select(ToDto).ToList(),
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            });
        }

        public async Task<ServiceResult<AnnouncementBarDto>> Handle(
            GetAnnouncementBarByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            return entity is null
                ? ServiceResult<AnnouncementBarDto>.Failed("نوار اعلان یافت نشد")
                : ServiceResult<AnnouncementBarDto>.Ok(ToDto(entity));
        }

        public async Task<ServiceResult<AnnouncementBarDto>> Handle(
            GetCurrentAnnouncementBarQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetCurrentAsync(DateTime.UtcNow, cancellationToken);
            // موفق با Data = null: «الان نواری برای نمایش نیست» خطا نیست.
            return ServiceResult<AnnouncementBarDto>.Ok(entity is null ? null! : ToDto(entity));
        }

        internal static AnnouncementBarDto ToDto(AnnouncementBar b) => new()
        {
            Id = b.Id,
            IsActive = b.IsActive,
            MessageFa = b.MessageFa,
            MessageEn = b.MessageEn,
            LinkUrl = b.LinkUrl,
            BackgroundImageUrl = b.BackgroundImageUrl,
            BackgroundColor = b.BackgroundColor,
            TextColor = b.TextColor,
            HeightPx = b.HeightPx,
            StartsAt = b.StartsAt,
            EndsAt = b.EndsAt,
            DisplayOrder = b.DisplayOrder,
        };
    }
}
