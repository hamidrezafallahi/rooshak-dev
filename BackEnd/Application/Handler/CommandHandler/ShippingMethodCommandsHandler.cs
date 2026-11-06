using Application.Commands;
using Application.Common;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class ShippingMethodCommandHandler(IShippingMethodRepository _repo, IHttpContextAccessor _accessor) :
        IRequestHandler<CreateShippingMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateShippingMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveShippingMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteShippingMethodCommand, ServiceResult<IdDto>>,
        IRequestHandler<SetDefaultShippingMethodCommand, ServiceResult<IdDto>>
    {


        public async Task<ServiceResult<IdDto>> Handle(CreateShippingMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var method = ShippingMethod.Create(
                request.Title,
                request.Description ?? string.Empty,
                request.EstimatedDeliveryTime,
                request.Price,
                request.IsDefault ?? false,
                userId.Value
            );
            await _repo.AddAsync(method);
            await _repo.SaveChangesAsync(cancellationToken);

            if (request.IsDefault == true)
            {
                var activeMethods = await _repo.GetAllAsync();
                foreach (var m in activeMethods)
                {
                    m.SetDefault(m.Id == method.Id, userId.Value);
                }
                await _repo.SaveChangesAsync(cancellationToken);
            }

            return ServiceResult<IdDto>.Ok(new IdDto { Id = method.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(UpdateShippingMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var method = await _repo.GetByIdAsync(request.Id);
            if (method == null)
                return ServiceResult<IdDto>.Failed("نحوه ارسال  یافت نشد");
            method.Update(
                request.Title,
                request.Description,
                request.EstimatedDeliveryTime,
                request.Price,
                request.IsDefault ?? false,
                userId.Value
            );
            if (request.IsDefault == true)
            {
                var activeMethods = await _repo.GetAllAsync();

                foreach (var m in activeMethods)
                {
                    m.SetDefault(m.Id == method.Id, userId.Value);
                }
            }
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = method.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveShippingMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var method = await _repo.GetByIdAsync(request.Id);
            if (method == null)
                return ServiceResult<IdDto>.Failed("متد ارسال پیدا نشد");
            method.SetActive(request.IsActive, userId.Value);

            await _repo.SaveChangesAsync();
            return ServiceResult<IdDto>.Ok(new IdDto { Id = method.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteShippingMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();
            var method = await _repo.GetByIdAsync(request.Id);
            if (method == null)
                return ServiceResult<IdDto>.Failed("متد ارسال پیدا نشد");
            method.Delete(userId.Value);

            await _repo.SaveChangesAsync();
            return ServiceResult<IdDto>.Ok(new IdDto { Id = method.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(SetDefaultShippingMethodCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var activeMethods = await _repo.GetAllAsync();
            if (!activeMethods.Any())
                return ServiceResult<IdDto>.Failed("روش ارسالی یافت نشد");

            // همه رو غیرفعال می‌کنیم
            foreach (var method in activeMethods)
            {
                method.SetDefault(method.Id == request.ShippingMethodId, userId.Value);
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = request.ShippingMethodId });
        }
    }
}
