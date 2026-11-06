using Application.Common;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;

public class ProductTagCommandHandler(IProductOfferTagRepository _repository, IHttpContextAccessor _accessor)
    : IRequestHandler<CreateProductOfferTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<UpdateProductOfferTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<ActiveProductOfferTagCommand, ServiceResult<IdDto>>,
    IRequestHandler<DeleteProductOfferTagCommand, ServiceResult<IdDto>>
{
   
    public async Task<ServiceResult<IdDto>> Handle(CreateProductOfferTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var productOfferTag = ProductOfferTag.Create(request.ProductOfferId, request.TagId, userId.Value);
        var exist = _repository.Query(pot => pot.TagId == request.TagId && pot.ProductOfferId == request.ProductOfferId).FirstOrDefault();
        if (exist is not null)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این پیشنهاد محصول اضافه شده است.");
        }
        await _repository.AddAsync(productOfferTag);
        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = productOfferTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(UpdateProductOfferTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var productTag = await _repository.GetByIdAsync(request.Id);
        if (productTag == null) return ServiceResult<IdDto>.Failed("برچسب پیشنهاد محصول پیدا نشد.");
        if (productTag.ProductOfferId == request.ProductOfferId && productTag.TagId == request.TagId)
        {
            return ServiceResult<IdDto>.Failed("این برچسب قبلاً به این پیشنهاد محصول اضافه شده است.");
        }

        productTag.Update(request.ProductOfferId, request.TagId, userId.Value);

        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = productTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(ActiveProductOfferTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var productTag = await _repository.GetByIdAsync(request.Id);
        if (productTag == null) return ServiceResult<IdDto>.Failed("برچسب پیشنهاد محصول پیدا نشد.");

        productTag.SetActive(request.IsActive, userId.Value);

        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = productTag.Id });
    }
    public async Task<ServiceResult<IdDto>> Handle(DeleteProductOfferTagCommand request, CancellationToken cancellationToken)
    {
        var userId = _accessor.HttpContext.GetUserId();
        if (userId == null)
            return ServiceResult<IdDto>.Unauthorized();
        var productTag = await _repository.GetByIdAsync(request.Id);
        if (productTag == null) return ServiceResult<IdDto>.Failed("برچسب پیشنهاد محصول پیدا نشد.");

        productTag.Delete(userId.Value);

        await _repository.SaveChangesAsync(cancellationToken);

        return ServiceResult<IdDto>.Ok(new IdDto { Id = productTag.Id });
    }
}
 