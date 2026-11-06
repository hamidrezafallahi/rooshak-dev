using Application.Commands;
using Application.Common;
using Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class ProductSpecificationCommandHandler(
            IProductSpecificationRepository _repo,
            IHttpContextAccessor _accessor) :
        IRequestHandler<CreateProductSpecificationCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateProductSpecificationCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveProductSpecificationCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteProductSpecificationCommand, ServiceResult<IdDto>>

    {


        public async Task<ServiceResult<IdDto>> Handle(CreateProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();


            var Specification = ProductSpecification.Create(
                 request.ProductId,
            request.Key,
            request.Value);

            await _repo.AddAsync(Specification);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = Specification.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var Specification = await _repo.GetByIdAsync(request.Id ?? 0);
            if (Specification == null)
                return ServiceResult<IdDto>.Failed("مشخصات محصول پیدا نشد");
            Specification.Update(
             request.Key,
             request.Value,
             userId.Value);

            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = Specification.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(ActiveProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var Specification = await _repo.GetByIdAsync(request.Id);
            if (Specification == null)
                return ServiceResult<IdDto>.Failed("مشخصات محصول پیدا نشد");

            Specification.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = Specification.Id });
        }
        public async Task<ServiceResult<IdDto>> Handle(DeleteProductSpecificationCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var Specification = await _repo.GetByIdAsync(request.Id);
            if (Specification == null)
                return ServiceResult<IdDto>.Failed("مشخصات محصول پیدا نشد");

            Specification.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = Specification.Id });
        }


    }



}
