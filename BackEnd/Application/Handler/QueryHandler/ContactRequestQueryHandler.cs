using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class ContactRequestQueryHandler(
        IContactRequestRepository _repo,
        IEntityConfigRepository _configRepo,
        IHttpContextAccessor _accessor) :
        IRequestHandler<GetAllContactRequestsQuery, ServiceResult<ListDto<ContactRequestDto>>>,
        IRequestHandler<GetContactRequestByIdQuery, ServiceResult<ContactRequestDto>>
    {
        public async Task<ServiceResult<ListDto<ContactRequestDto>>> Handle(GetAllContactRequestsQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<ContactRequest> query = _repo.Query();

            if (request.IsReviewed.HasValue)
            {
                var isReviewed = request.IsReviewed.Value;
                query = query.Where(c => c.IsReviewed == isReviewed);
            }

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim();
                query = query.Where(c =>
                    c.FirstName.Contains(q) ||
                    c.LastName.Contains(q) ||
                    c.PhoneNumber.Contains(q) ||
                    (c.Email != null && c.Email.Contains(q)));
            }

            int totalCount = await query.CountAsync(cancellationToken);

            // جدیدترین درخواست ها بالاتر
            var records = await query
                .OrderByDescending(c => c.Id)
                .Select(c => new ContactRequestDto
                {
                    Id = c.Id,
                    IsActive = c.IsActive,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Address = c.Address,
                    PreferredContactTime = c.PreferredContactTime,
                    Message = c.Message,
                    IsReviewed = c.IsReviewed,
                    AdminNote = c.AdminNote,
                    CreatedAt = c.CreatedAt
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            EntityConfig? config = null;
            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("contactRequests");
            }

            var result = new ListDto<ContactRequestDto>
            {
                Records = records,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            return ServiceResult<ListDto<ContactRequestDto>>.Ok(result);
        }

        public async Task<ServiceResult<ContactRequestDto>> Handle(GetContactRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<ContactRequestDto>.Failed("Unauthorized");

            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null)
                return ServiceResult<ContactRequestDto>.Failed("درخواست یافت نشد");

            var dto = new ContactRequestDto
            {
                Id = entity.Id,
                IsActive = entity.IsActive,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                Address = entity.Address,
                PreferredContactTime = entity.PreferredContactTime,
                Message = entity.Message,
                IsReviewed = entity.IsReviewed,
                AdminNote = entity.AdminNote,
                CreatedAt = entity.CreatedAt
            };
            return ServiceResult<ContactRequestDto>.Ok(dto);
        }
    }
}
