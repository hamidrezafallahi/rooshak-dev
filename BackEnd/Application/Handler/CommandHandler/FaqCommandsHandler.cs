using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class FaqCommandHandler(IFaqRepository _repo, IHttpContextAccessor _accessor) :
        IRequestHandler<CreateFaqCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateFaqCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveFaqCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteFaqCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            if (string.IsNullOrWhiteSpace(request.Question))
                return ServiceResult<IdDto>.Failed("متن سوال الزامی است");

            if (string.IsNullOrWhiteSpace(request.Answer))
                return ServiceResult<IdDto>.Failed("متن پاسخ الزامی است");

            var faq = Faq.Create(
                request.Question,
                request.Answer,
                request.DisplayOrder ?? 0,
                userId.Value
            );

            await _repo.AddAsync(faq);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = faq.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var faq = await _repo.GetByIdAsync(request.Id);
            if (faq == null)
                return ServiceResult<IdDto>.Failed("سوال متداول یافت نشد");

            faq.Update(
                request.Question,
                request.Answer,
                request.DisplayOrder,
                userId.Value
            );

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = faq.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveFaqCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var faq = await _repo.GetByIdAsync(request.Id);
            if (faq == null)
                return ServiceResult<IdDto>.Failed("سوال متداول یافت نشد");

            faq.SetActive(request.IsActive, userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = faq.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var faq = await _repo.GetByIdAsync(request.Id);
            if (faq == null)
                return ServiceResult<IdDto>.Failed("سوال متداول یافت نشد");

            faq.Delete(userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = faq.Id });
        }
    }
}
