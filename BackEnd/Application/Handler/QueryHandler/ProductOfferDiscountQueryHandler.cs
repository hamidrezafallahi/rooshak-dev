using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

public class ProductOfferDiscountQueryHandler(IProductOfferDiscountRepository _repository, IEntityConfigRepository _configRepo)
        : IRequestHandler<GetAllProductOfferDiscountQuery, ServiceResult<ListDto<ProductOfferDiscountDto>>>,
        IRequestHandler<GetProductOfferDiscountByIdQuery, ServiceResult<ProductOfferDiscountDto>>,
        IRequestHandler<GetProductOfferDiscountByProductIdQuery, ServiceResult<IEnumerable<ProductOfferDiscountDto>>>,
        IRequestHandler<GetAllProductOfferDiscountByDiscountIdQuery, ServiceResult<IEnumerable<ProductOfferDiscountDto>>>
    {
        public async Task<ServiceResult<ListDto<ProductOfferDiscountDto>>> Handle(GetAllProductOfferDiscountQuery request,CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<ProductOfferDiscount> query;
        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repository.Query()
            .Include(pd => pd.ProductOffer).ThenInclude(po => po.Supplier)
            .Include(pd => pd.ProductOffer).ThenInclude(po => po.Product).ThenInclude(p=>p.Images)
            .Include(pd => pd.Discount);
        }
        else
        {
            query = _repository.Query(b => b.IsActive)
             .Include(pd => pd.ProductOffer).ThenInclude(po => po.Supplier)
        .Include(pd => pd.ProductOffer).ThenInclude(po => po.Product).ThenInclude(p => p.Images)
        .Include(pd => pd.Discount);
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedproductOfferDiscount = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var productOfferDiscountsDto = pagedproductOfferDiscount.Select(pd => new ProductOfferDiscountDto
        {
            Id = pd.Id,
            ProductName = pd.ProductOffer.Product.Name,
            ProductImage =pd.ProductOffer.Product.Images.Where(i => i.IsMain && !i.IsDeleted).Select(i => i.ImageUrl).First().TrimStart('/'),
            Supplier = pd.ProductOffer.Supplier.FullName,
            DiscountTitle = pd.Discount.Title,
            DiscountAmount = pd.Discount.Amount,
            DiscountIsPercent = pd.Discount.IsPercent,
            IsActive=pd.IsActive
 
        }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("productOfferDiscounts");
        }

        var resultDto = new ListDto<ProductOfferDiscountDto>
        {
            Records = productOfferDiscountsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<ProductOfferDiscountDto>>.Ok(resultDto);
        }
        public async Task<ServiceResult<ProductOfferDiscountDto>> Handle(GetProductOfferDiscountByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = _repository.Query(pod => pod.Id == request.Id)
                .Include(pod => pod.Discount)
                .Include(pod => pod.ProductOffer)
                    .ThenInclude(po => po.Product)
                .Select(pod => new ProductOfferDiscountDto
                {
                    Id = pod.Id,
                    ProductOfferId = pod.ProductOfferId,
                    ProductId=pod.ProductOffer.ProductId,
                    DiscountId = pod.DiscountId,
                    CreatedAt = pod.CreatedAt,
                    DiscountAmount = pod.Discount.Amount,
                    DiscountIsPercent = pod.Discount.IsPercent,
                    DiscountStartDate = pod.Discount.StartDate,
                    DiscountEndDate = pod.Discount.EndDate,
                    DiscountTitle = pod.Discount.Title,
                    ProductName = pod.ProductOffer.Product.Name

                }).FirstOrDefault();
            if (dto == null) return ServiceResult<ProductOfferDiscountDto>.Failed("تخفیف این پیشنهاد محصول پیدا نشد.");

            return ServiceResult<ProductOfferDiscountDto>.Ok(dto);
        }
        public async Task<ServiceResult<IEnumerable<ProductOfferDiscountDto>>> Handle(GetProductOfferDiscountByProductIdQuery request, CancellationToken cancellationToken)
        {
            var productOfferDiscounts = await _repository
                .Query(pd => pd.ProductOfferId == request.ProductOfferId && !pd.IsDeleted)
                .ToListAsync(cancellationToken);

            var dtos = productOfferDiscounts.Select(pd => new ProductOfferDiscountDto
            {
                Id = pd.Id,
                ProductOfferId = pd.ProductOfferId,
                DiscountId = pd.DiscountId,
                CreatedAt = pd.CreatedAt
            }).ToList();

            return ServiceResult<IEnumerable<ProductOfferDiscountDto>>.Ok(dtos);
        }
        public async Task<ServiceResult<IEnumerable<ProductOfferDiscountDto>>> Handle(GetAllProductOfferDiscountByDiscountIdQuery request, CancellationToken cancellationToken)
        {
            var productOfferDiscounts = await _repository
                .Query(pd => pd.DiscountId == request.DiscountId && !pd.IsDeleted)
                .ToListAsync(cancellationToken);

            var dtos = productOfferDiscounts.Select(pd => new ProductOfferDiscountDto
            {
                Id = pd.Id,
                ProductOfferId = pd.ProductOfferId,
                DiscountId = pd.DiscountId,
                CreatedAt = pd.CreatedAt
            }).ToList();

            return ServiceResult<IEnumerable<ProductOfferDiscountDto>>.Ok(dtos);
        }
    }
 