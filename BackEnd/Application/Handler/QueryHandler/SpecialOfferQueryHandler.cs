using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class GetSpecialOffersQueryHandler(
            ISpecialOfferRepository _repo,
            IEntityConfigRepository _configRepo) :
        IRequestHandler<GetSpecialOffersQuery, ServiceResult<ListDto<SpecialOffersDto>>>,
        IRequestHandler<GetLandingSpecialOffersQuery, ServiceResult<ListDto<LandingSpecialOffersDto>>>,
        IRequestHandler<GetSpecialOfferByIdQuery, ServiceResult<SpecialOffersDto?>>
{
    public async Task<ServiceResult<ListDto<SpecialOffersDto>>> Handle(GetSpecialOffersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<SpecialOffer> query;

        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query()
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.ProductOffer).ThenInclude(po => po.Supplier)
                .Include(s => s.Discount)
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.EndDate);
        }
        else
        {
            query = _repo.Query(s => s.IsActive)
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.ProductOffer).ThenInclude(po => po.Supplier)
                .Include(s => s.Discount)
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.EndDate);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var pagedEntity = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var specialsDto = pagedEntity.Select(s => new SpecialOffersDto

        {
            Id = s.Id,
            DisplayOrder = s.DisplayOrder,
            StartDate = s.StartDate,
            EndDate=s.EndDate,
            IsActive = s.IsActive,
            ProductName = s.ProductOffer.Product.Name,
            SupplierName=s.ProductOffer.Supplier.FullName,
            ProductImage = s.ProductOffer.Product.Images
                    .Where(i => !i.IsDeleted && i.IsMain)
                    .Select(i => i.ImageUrl.TrimStart('/'))
                    .FirstOrDefault(),
            DiscountName = s.Discount.Title


        })
        .ToList();

        dynamic? config = null;
        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("specialOffers");
        }

        var resultDto = new ListDto<SpecialOffersDto>
        {
            Records = specialsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<SpecialOffersDto>>.Ok(resultDto);
    }

    public async Task<ServiceResult<ListDto<LandingSpecialOffersDto>>> Handle(GetLandingSpecialOffersQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<SpecialOffer> query;

        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query(s => s.EndDate >= now && s.StartDate <= now).Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.ProductOffers).ThenInclude(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Discounts)
                        .ThenInclude(pd => pd.Discount)
                .Include(s => s.Discount)
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.EndDate);
        }
        else
        {
            query = _repo.Query(s => s.IsActive && !s.IsDeleted &&
                  s.StartDate <= now && s.EndDate >= now).Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.ProductOffers).ThenInclude(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Product)
                        .ThenInclude(p => p.Images)
                .Include(s => s.ProductOffer)
                    .ThenInclude(po => po.Discounts)
                        .ThenInclude(pd => pd.Discount)
                .Include(s => s.Discount)
                .OrderBy(s => s.DisplayOrder)
                .ThenByDescending(s => s.EndDate); ;
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var specials = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var specialsDto = specials.Select(s =>
        {
            var offer = s.ProductOffer;
            if (offer == null) return null;

            var basePrice = offer.BasePrice;
            var finalPrice = offer.GetFinalPrice(now);

            // اگر تخفیف اختصاصی پیشنهاد ویژه وجود داشته باشد، می‌توان ترکیب کرد
            // اما فعلاً فقط از تخفیف‌های ProductOffer استفاده می‌کنیم
            var activeDiscount = offer.Discounts
                .Select(pd => pd.Discount)
                .FirstOrDefault(d => !d.IsDeleted && d.IsActive);

            var discountAmount = basePrice - finalPrice;

            var productDto = new ProductByDetailForSpecialsDto
            {
                Id = offer.Product.Id,
                ProductId = offer.Product.Id,
                ProductOfferId = offer.Id,
                Name = offer.Product.Name,
                Description = offer.Product.Description ?? "",
                Price = basePrice,
                FinalPrice = finalPrice,
                DiscountAmount = discountAmount > 0 ? discountAmount : null,
                DiscountId = activeDiscount?.Id ?? 0,
                DiscountIsPercent = activeDiscount?.IsPercent,

                BrandId = offer.Product.BrandId,
                CategoryId = offer.Product.CategoryId,

                Tags = offer.ProductOfferTags
                    .Where(pt => !pt.IsDeleted && pt.Tag != null && !pt.Tag.IsDeleted)
                    .Select(pt => new TagDto { Id = pt.Tag.Id, Name = pt.Tag.Name })
                    .ToList(),

                MainImage = offer.Product.Images
                    .Where(i => !i.IsDeleted && i.IsMain)
                    .Select(i => i.ImageUrl.TrimStart('/'))
                    .FirstOrDefault(),

                // اگر نیاز بود موجودی هم نمایش داده شود:
                // Inventory = offer.Inventory
            };

            return new LandingSpecialOffersDto
            {
                Id = s.Id,
                DisplayOrder = s.DisplayOrder,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                Product = productDto
            };
        })
        .Where(dto => dto != null)
        .ToList();

        dynamic? config = null;
        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("specialOffers");
        }

        var resultDto = new ListDto<LandingSpecialOffersDto>
        {
            Records = specialsDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<LandingSpecialOffersDto>>.Ok(resultDto);
    }
    public async Task<ServiceResult<SpecialOffersDto?>> Handle(GetSpecialOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var special = await _repo.Query(s => s.Id == request.Id && !s.IsDeleted)
            .Include(s => s.ProductOffer)
                .ThenInclude(po => po.Product)
                    .ThenInclude(p => p.Images)
            .Include(s => s.ProductOffer)
                .ThenInclude(po => po.Discounts)
                    .ThenInclude(pd => pd.Discount)
            .Include(s => s.ProductOffer)
                .ThenInclude(po=>po.Supplier)
            .Include(s => s.Discount)
            .FirstOrDefaultAsync(cancellationToken);

        if (special == null)
            return ServiceResult<SpecialOffersDto?>.Failed("پیشنهاد ویژه یافت نشد");

        var offer = special.ProductOffer;
        if (offer == null)
            return ServiceResult<SpecialOffersDto?>.Failed("پیشنهاد تامین‌کننده مرتبط یافت نشد");

        var basePrice = offer.BasePrice;
        var finalPrice = offer.GetFinalPrice(now);
        var discountAmount = basePrice - finalPrice;
        var productDto = new SpecialOffersDto
        {
            Id = special.Id,
            DiscountName= special.Discount.Title,
            DisplayOrder= special.DisplayOrder,
            EndDate= special.EndDate,
            StartDate= special.StartDate,
            IsActive= special.IsActive,
            ProductImage = offer.Product.Images
                .Where(i => !i.IsDeleted && i.IsMain)
                .Select(i => i.ImageUrl.TrimStart('/'))
                .FirstOrDefault(),
            ProductName= offer.Product.Name,
            ProductId=offer.Product.Id,
            ProductOfferId=offer.Id,
            DiscountId= special.Discount.Id,
            SupplierName =offer.Supplier.FullName

            //Name = offer.Product.Name,
            //Description = offer.Product.Description ?? "",
            //Price = basePrice,
            //FinalPrice = finalPrice,
            //DiscountAmount = discountAmount > 0 ? discountAmount : null,
            //DiscountId = activeDiscount?.Id??0,
            //DiscountIsPercent = activeDiscount?.IsPercent,
            //ProductId=offer.Product.Id,
            //ProductOfferId=offer.Id,
            //BrandId = offer.Product.BrandId,
            //CategoryId = offer.Product.CategoryId,
            //Tags = offer.ProductOfferTags
            //    .Where(pt => !pt.IsDeleted && pt.Tag != null && !pt.Tag.IsDeleted)
            //    .Select(pt => new TagDto { Id = pt.Tag.Id, Name = pt.Tag.Name })
            //    .ToList(),

            //MainImage = offer.Product.Images
            //    .Where(i => !i.IsDeleted && i.IsMain)
            //    .Select(i => $"{domainUrl}/{i.ImageUrl.TrimStart('/')}")
            //    .FirstOrDefault(),
        };

        

        return ServiceResult<SpecialOffersDto?>.Ok(productDto);
    }

}

