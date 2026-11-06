using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

    public class PaymentMethodCommandHandler(IPaymentMethodRepository _repo, IHttpContextAccessor _accessor) : 
        IRequestHandler<CreatePaymentMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdatePaymentMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActivePaymentMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeletePaymentMethodCommand, ServiceResult<IdDto>>
    {
       
        public async Task<ServiceResult<IdDto>> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Unauthorized();

            var entity = PaymentMethod.Create(
                request.Title,
                request.Code,
                request.IsOnline,
                request.ConfigJson,
                request.DisplayOrder,
                userId.Value
            );

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Unauthorized();

            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null) return ServiceResult<IdDto>.Failed("روش پرداخت موردنظر پیدا نشد.");

            entity.Update(
                request.Title ?? entity.Title,
                request.IsOnline ?? entity.IsOnline,
                request.ConfigJson ?? entity.ConfigJson,
                request.DisplayOrder ?? entity.DisplayOrder,
                userId.Value
            );

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value) entity.Activate(userId.Value);
                else entity.Deactivate(userId.Value);
            }

            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActivePaymentMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null) return ServiceResult<IdDto>.Unauthorized();
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null) return ServiceResult<IdDto>.Failed("روش پرداخت موردنظر پیدا نشد.");
        entity.SetActive(request.IsActive, userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);
        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
        }
    public async Task<ServiceResult<IdDto>> Handle(DeletePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null) return ServiceResult<IdDto>.Unauthorized();

        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null) return ServiceResult<IdDto>.Failed("روش پرداخت موردنظر پیدا نشد.");

        entity.Delete(userId.Value);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = entity.Id });
    }
}
