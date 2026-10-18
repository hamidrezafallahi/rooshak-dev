using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class ProductOfferQueryHandler(IProductOfferRepository _offerRepo,IEntityConfigRepository _configRepo) :
        IRequestHandler<GetProductOffersQuery, ServiceResult<ListDto<ProductOfferDetailDto?>>>,
        IRequestHandler<GetProductOffers4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
        IRequestHandler<GetProductOfferByIdQuery, ServiceResult<ProductOfferDetailDto?>>,
        IRequestHandler<GetProductOffersByProductIdQuery, ServiceResult<List<ProductOfferDto>>>,
        IRequestHandler<GetProductOffersBySellerIdQuery, ServiceResult<List<ProductOfferDto>>>,
        IRequestHandler<GetSuppliersQuery, ServiceResult<SupplierListDto>>,
        IRequestHandler<GetSuppliersByCategoryIdQuery, ServiceResult<SupplierListDto>>,
        IRequestHandler<GetSuppliersIdsQuery, ServiceResult<List<IdDto?>>>
{

    public async Task<ServiceResult<ListDto<ProductOfferDetailDto?>>> Handle(GetProductOffersQuery request, CancellationToken cancellationToken)
    {

        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        var now = DateTime.UtcNow;
        IQueryable<ProductOffers> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _offerRepo.Query()
                    .Include(po => po.Product)
                    .Include(po => po.Supplier)
                    .Include(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                    .Include(po => po.Discounts).ThenInclude(d => d.Discount)
                    .Where(po => po.Product.Name.Contains(request.Q));
            }
            else
            {
                query = _offerRepo.Query(po => po.IsActive)
                    .Include(po => po.Product)
                    .Include(po => po.Supplier)
                    .Include(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                    .Include(po => po.Discounts).ThenInclude(d => d.Discount)
                    .Where(po => po.Product.Name.Contains(request.Q));
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _offerRepo.Query()
                    .Include(po => po.Product)
                    .Include(po => po.Supplier)
                    .Include(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                    .Include(po => po.Discounts).ThenInclude(d => d.Discount);
            }
            else
            {
                query = _offerRepo.Query(b => b.IsActive)
                    .Include(po => po.Product)
                    .Include(po => po.Supplier)
                    .Include(po => po.ProductOfferTags).ThenInclude(pt => pt.Tag)
                    .Include(po => po.Discounts).ThenInclude(d => d.Discount);
            }
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedProductOffers = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var ProductOffersDto = pagedProductOffers.Select(offer => new ProductOfferDetailDto
        {
            Id = offer.Id,
            ProductId = offer.ProductId,
            ProductName = offer.Product.Name,
            SupplierId = offer.SupplierId,
            SupplierSlug = offer.Supplier?.Slug,
            SupplierName = offer.Supplier.FullName,
            SupplierImage = string.IsNullOrEmpty(offer.Supplier.Image)
                ? string.Empty
                : offer.Supplier.Image.TrimStart('/'),
            BasePrice = offer.BasePrice,
            FinalPrice = offer.GetFinalPrice(now),
            Inventory = offer.Inventory,
            IsActive = offer.IsActive,
            CreatedAt = offer.CreatedAt,
            Tags = offer.ProductOfferTags
                .Where(pot => pot.Tag != null && pot.Tag.IsActive)
                .Select(pot => new TagDto { Name = pot.Tag.Name, Id = pot.Tag.Id, Slug = pot.Tag.Slug, IsActive = pot.Tag.IsActive })
                .ToList()
        }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("productOffers");
        }

        var resultDto = new ListDto<ProductOfferDetailDto>
        {
            Records = ProductOffersDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<ProductOfferDetailDto>>.Ok(resultDto);



    }
    public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetProductOffers4selectOptionQuery request, CancellationToken ct)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        int productId = request.ProductId ?? 0;

        IQueryable<ProductOffers> query;
        if (productId == 0)
        {
            query = _offerRepo.Query(x => x.IsActive).Include(x => x.Product).Include(x => x.Supplier);
        }
        else
        {
            query = _offerRepo.Query(c => c.IsActive && (c.ProductId == productId)).Include(x => x.Product).Include(x => x.Supplier);
        }
        int totalCount = await query.CountAsync(c => c.IsActive);
        var pagedProductOffers = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        var flatDtos = pagedProductOffers.Select(c => new SelectOptionDto
        {
            Id = c.Id,
            PersianLabel = $"{c.Product.Name} - {c.Supplier.FullName}",
            EnglishLabel = $"{c.Product.Name} - {c.Supplier.FullName}"
        }).ToList();
        var resultDto = new ListDto<SelectOptionDto>
        {
            Records = flatDtos,
            ColumnsJson = null,
            ActionsJson = null,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };
        return ServiceResult<ListDto<SelectOptionDto>>.Ok(resultDto);

    }

    public async Task<ServiceResult<ProductOfferDetailDto?>> Handle(GetProductOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var offer = await _offerRepo.GetByIdWithDetailsAsync(request.Id);
        if (offer == null || offer.IsDeleted)
            return ServiceResult<ProductOfferDetailDto?>.Ok(null);

        var dto = new ProductOfferDetailDto
        {
            Id = offer.Id,
            ProductId = offer.ProductId,
            ProductName = offer.Product?.Name ?? "",
            SupplierId = offer.SupplierId,
            SupplierSlug = offer.Supplier?.Slug,
            SupplierName = offer.Supplier?.FullName ?? "",
            BasePrice = offer.BasePrice,
            FinalPrice = offer.GetFinalPrice(now),
            Inventory = offer.Inventory,
            IsActive = offer.IsActive,
            CreatedAt = offer.CreatedAt,
            AllDiscounts = offer.Discounts
                .Where(pd => !pd.IsDeleted && !pd.Discount.IsDeleted)
                .Select(pd => new DiscountDto
                {
                    Id = pd.Discount.Id,
                    Title = pd.Discount.Title,
                    Amount = pd.Discount.Amount,
                    IsPercent = pd.Discount.IsPercent,
                    StartDate = pd.Discount.StartDate,
                    EndDate = pd.Discount.EndDate,
                    IsActive = pd.Discount.IsActive
                })
                .ToList(),
            ActiveDiscounts = offer.Discounts
                .Where(pd => pd.IsActive && !pd.IsDeleted &&
                            !pd.Discount.IsDeleted &&
                            pd.Discount.IsActive &&
              pd.Discount.StartDate <= now && pd.Discount.EndDate >= now)
                .Select(pd => new DiscountDto
                {
                    Id = pd.Discount.Id,
                    Title = pd.Discount.Title,
                    Amount = pd.Discount.Amount,
                    IsPercent = pd.Discount.IsPercent,
                    StartDate = pd.Discount.StartDate,
                    EndDate = pd.Discount.EndDate,
                    IsActive = pd.Discount.IsActive
                })
                .ToList()
        };

        return ServiceResult<ProductOfferDetailDto?>.Ok(dto);
    }
    public async Task<ServiceResult<List<ProductOfferDto>>> Handle(GetProductOffersByProductIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var offers = await _offerRepo.Query().Include(o => o.Product)
                .Where(o => o.ProductId == request.ProductId && o.IsActive && !o.IsDeleted)
            .Include(o => o.Supplier).ToListAsync();

        var dtos = offers.Select(offer => new ProductOfferDto
        {
            Id = offer.Id,
            ProductId = offer.ProductId,
            ProductSlug = offer.Product?.Slug,
            ProductName = offer.Product?.Name ?? "",
            SupplierId = offer.SupplierId,
            SupplierSlug = offer.Supplier?.Slug,
            SupplierName = offer.Supplier?.FullName ?? "",
            SupplierImage = offer.Supplier.Image.TrimStart('/'),
            SupplierDesc = offer.Supplier.UserDescription,
            BasePrice = offer.BasePrice,
            FinalPrice = offer.GetFinalPrice(now),
            Inventory = offer.Inventory,
            IsActive = offer.IsActive,
            CreatedAt = offer.CreatedAt,
            ActiveDiscounts = offer.Discounts
                     .Where(pd => pd.IsActive && !pd.IsDeleted &&
                                !pd.Discount.IsDeleted &&
                                pd.Discount.IsActive &&
                  pd.Discount.StartDate <= now && pd.Discount.EndDate >= now)
                    .Select(pd => new DiscountDto
                    {
                        Id = pd.Discount.Id,
                        Title = pd.Discount.Title,
                        Amount = pd.Discount.Amount,
                        IsPercent = pd.Discount.IsPercent,
                        StartDate = pd.Discount.StartDate,
                        EndDate = pd.Discount.EndDate,
                        IsActive = pd.Discount.IsActive
                    })
                    .ToList()
        }).ToList();

        return ServiceResult<List<ProductOfferDto>>.Ok(dtos);
    }
    public async Task<ServiceResult<List<ProductOfferDto>>> Handle(GetProductOffersBySellerIdQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var offers = _offerRepo.Query(offer => offer.IsActive).Include(o => o.Supplier).Include(o => o.Product).ToList();

        var dtos = offers.Select(offer => new ProductOfferDto
        {
            Id = offer.Id,
            ProductId = offer.ProductId,
            ProductName = offer.Product?.Name ?? "",
            ProductDescription=offer.Product.Description,
            ProductImage =offer.Supplier.Image.TrimStart('/'),
            SupplierId = offer.SupplierId,
            SupplierSlug = offer.Supplier?.Slug,
            SupplierName = offer.Supplier?.FullName ?? "",
            SupplierImage = offer.Supplier.Image.TrimStart('/'),
            SupplierDesc = offer.Supplier.UserDescription,
            BasePrice = offer.BasePrice,
            FinalPrice = offer.GetFinalPrice(now),
            Inventory = offer.Inventory,
            IsActive = offer.IsActive,
            CreatedAt = offer.CreatedAt,
            ActiveDiscounts = offer.Discounts
                 .Where(pd => pd.IsActive && !pd.IsDeleted &&
                            !pd.Discount.IsDeleted &&
                            pd.Discount.IsActive &&
              pd.Discount.StartDate <= now && pd.Discount.EndDate >= now)
                .Select(pd => new DiscountDto
                {
                    Id = pd.Discount.Id,
                    Title = pd.Discount.Title,
                    Amount = pd.Discount.Amount,
                    IsPercent = pd.Discount.IsPercent,
                    StartDate = pd.Discount.StartDate,
                    EndDate = pd.Discount.EndDate,
                    IsActive = pd.Discount.IsActive
                })
                .ToList()
        }).ToList();

        return ServiceResult<List<ProductOfferDto>>.Ok(dtos);
    }
    public async Task<ServiceResult<SupplierListDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;

        var query = _offerRepo.Query(o => !o.IsDeleted && o.IsActive)
            .GroupBy(o => o.SupplierId)
            .Select(g => new SupplierDto
            {
                Id = g.Key,
                Slug = g.First().Supplier.Slug,
                FullName = g.First().Supplier.FullName,
                Image = g.First().Supplier.Image,
                UserDescription = g.First().Supplier.UserDescription
            });

        int totalCount = await query.CountAsync(cancellationToken);

        var pagedSuppliers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        foreach (var supplier in pagedSuppliers)
        {
            if (!string.IsNullOrEmpty(supplier.Image))
                supplier.Image = supplier.Image.TrimStart('/');
        }

        dynamic? config = null;
        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("users");
        }

        var supplierList = new SupplierListDto
        {
            Records = pagedSuppliers,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalCount = totalCount,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
        };

        return ServiceResult<SupplierListDto>.Ok(supplierList);
    }
    public async Task<ServiceResult<SupplierListDto>> Handle(GetSuppliersByCategoryIdQuery request, CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;

        var query = _offerRepo.Query(o => !o.IsDeleted && o.IsActive && o.Product.CategoryId == request.CategoryId)
            .GroupBy(o => o.SupplierId)
            .Select(g => new SupplierDto
            {
                Id = g.Key,
                Slug = g.First().Supplier.Slug,
                FullName = g.First().Supplier.FullName,
                Image = g.First().Supplier.Image,
                UserDescription = g.First().Supplier.UserDescription
            });

        int totalCount = await query.CountAsync(cancellationToken);

        var pagedSuppliers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        foreach (var supplier in pagedSuppliers)
        {
            if (!string.IsNullOrEmpty(supplier.Image))
                supplier.Image = supplier.Image.TrimStart('/');
        }

        dynamic? config = null;
        //if (request.ByConfig == true)
        //{
        //    config = await _configRepo.GetByEntityNameAsync("brands");
        //}

        var supplierList = new SupplierListDto
        {
            Records = pagedSuppliers,
            PageSize = pageSize,
            PageNumber = pageNumber,
            TotalCount = totalCount,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
        };

        return ServiceResult<SupplierListDto>.Ok(supplierList);
    }
    public async Task<ServiceResult<List<IdDto?>>> Handle(GetSuppliersIdsQuery request, CancellationToken cancellationToken)
    {
        var suppliersIds = await _offerRepo
  .Query(o => !o.IsDeleted && o.IsActive)
  .GroupBy(o => o.SupplierId)
  .Select(g => g.Select(s => new IdDto
  {
      Id = s.SupplierId,
  }).First())
  .ToListAsync(cancellationToken);


        return ServiceResult<List<IdDto?>>.Ok(suppliersIds);
    }

}

