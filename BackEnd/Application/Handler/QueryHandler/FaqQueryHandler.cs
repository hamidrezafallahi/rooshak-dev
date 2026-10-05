using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class FaqQueryHandler(IFaqRepository _repo, IEntityConfigRepository _configRepo) :
        IRequestHandler<GetAllFaqsQuery, ServiceResult<ListDto<FaqDto>>>,
        IRequestHandler<GetFaqByIdQuery, ServiceResult<FaqDto>>
    {
        public async Task<ServiceResult<ListDto<FaqDto>>> Handle(GetAllFaqsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<Faq> query = _repo.Query();

            // پیش فرض: فقط موارد فعال (برای نمایش عمومی)
            if (request.OnlyActives.HasValue && request.OnlyActives == true)
                query = query.Where(f => f.IsActive);

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(f => f.Question.Contains(q) || f.Answer.Contains(q));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            var records = await query
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.Id)
                .Select(f => new FaqDto
                {
                    Id = f.Id,
                    IsActive = f.IsActive,
                    Question = f.Question,
                    Answer = f.Answer,
                    DisplayOrder = f.DisplayOrder
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            EntityConfig? config = null;
            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("faqs");
            }

            var result = new ListDto<FaqDto>
            {
                Records = records,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            return ServiceResult<ListDto<FaqDto>>.Ok(result);
        }

        public async Task<ServiceResult<FaqDto>> Handle(GetFaqByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null)
                return ServiceResult<FaqDto>.Failed("سوال متداول یافت نشد");

            var dto = new FaqDto
            {
                Id = entity.Id,
                IsActive = entity.IsActive,
                Question = entity.Question,
                Answer = entity.Answer,
                DisplayOrder = entity.DisplayOrder
            };
            return ServiceResult<FaqDto>.Ok(dto);
        }
    }
}
