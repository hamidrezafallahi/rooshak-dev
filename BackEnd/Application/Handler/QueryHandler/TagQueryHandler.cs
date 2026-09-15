using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

public class TagQueryHandler(ITagRepository _repo, IProductOfferTagRepository _productOfferRepository, IProductRepository _productRepository, IEntityConfigRepository _configRepo)
    : IRequestHandler<GetTagsQuery, ServiceResult<ListDto<TagDto>>>,
    IRequestHandler<GetTags4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
    IRequestHandler<GetTagBySlugQuery, ServiceResult<TagDto?>>,
    IRequestHandler<GetTagsByProductOfferIdQuery, ServiceResult<IEnumerable<TagDto>>>,
    IRequestHandler<GetAllTagIdsQuery, ServiceResult<List<IdDto>>>,
    IRequestHandler<GetAllTagsSlugsQuery, ServiceResult<IEnumerable<SlugDto>>>,
    IRequestHandler<GetTagPriceListQuery, ServiceResult<TagPriceListDto>>,
    IRequestHandler<GetTagFamiliesQuery, ServiceResult<IEnumerable<TagFamilyDto>>>
{
     
    public async Task<ServiceResult<ListDto<TagDto>>> Handle(GetTagsQuery request,CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Tag> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query(b => b.Name.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive && b.Name.Contains(request.Q));
            }
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(b => b.IsActive);
            }
        }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedTags = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var dtos = pagedTags.Select(x => new TagDto
        {
            Id = x.Id,
            Name = x.Name,
            Slug = x.Slug,
            IsActive=x.IsActive
          
        }).ToList();



        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("tags");
        }
        var resultDto = new ListDto<TagDto>
        {
            Records = dtos,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<TagDto>>.Ok(resultDto);

     
        }
    public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetTags4selectOptionQuery request, CancellationToken ct)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;


        IQueryable<Tag> query;
        query = _repo.Query(c => c.IsActive);
        int totalCount = await query.CountAsync(c => c.IsActive);
        var pagedEntity = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        var flatDtos = pagedEntity.Select(x => new SelectOptionDto
        {
            Id = x.Id,
            PersianLabel = x.Name,
            EnglishLabel = x.Name
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

    public async Task<ServiceResult<TagDto?>> Handle(GetTagBySlugQuery request, CancellationToken cancellationToken)
    {
        Tag? tag = null;
        if (int.TryParse(request.Slug, out int tagId))
        {
            tag = await _repo.Query(t => t.Id == tagId && !t.IsDeleted)
                             .FirstOrDefaultAsync(cancellationToken);
        }
        else
        {
            tag = await _repo.Query(t =>
                    (t.Slug == request.Slug || t.Name == request.Slug) && !t.IsDeleted)
                             .FirstOrDefaultAsync(cancellationToken);
        }

        if (tag == null)
            return ServiceResult<TagDto?>.Failed("Tag not found");

        var dto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            IsActive = tag.IsActive,
        };

        return ServiceResult<TagDto?>.Ok(dto);
    }
    public async Task<ServiceResult<IEnumerable<TagDto>>> Handle(GetTagsByProductOfferIdQuery request, CancellationToken cancellationToken)
    {
        var productTags = await _productOfferRepository
            .Query(pt => pt.ProductOfferId == request.ProductId && !pt.IsDeleted)
            .Include(pt => pt.Tag) // برای گرفتن اطلاعات تگ
            .ToListAsync(cancellationToken);

        var dtos = productTags
            .Where(pt => pt.Tag != null && !pt.Tag.IsDeleted)
            .Select(pt => new TagDto
            {
                Id = pt.Tag.Id,
                Name = pt.Tag.Name,
                Slug = pt.Tag.Slug,
            }).ToList();

        return ServiceResult<IEnumerable<TagDto>>.Ok(dtos);
    }
    public async Task<ServiceResult<List<IdDto>>> Handle(GetAllTagIdsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _repo
            .Query(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtos = tags.Select(t => new IdDto
        {
            Id = t.Id
        }).ToList();

        return ServiceResult<List<IdDto>>.Ok(dtos);
    }

    public async Task<ServiceResult<IEnumerable<SlugDto>>> Handle(GetAllTagsSlugsQuery request, CancellationToken cancellationToken)
    {
        var slugs = await _repo.Query(t => t.IsActive && !t.IsDeleted && !string.IsNullOrWhiteSpace(t.Slug))
            .Select(t => new SlugDto
            {
                Id = t.Id,
                Slug = t.Slug,
                UpdatedAt = t.UpdatedAt ?? t.CreatedAt,
            })
            .ToListAsync(cancellationToken);
        return ServiceResult<IEnumerable<SlugDto>>.Ok(slugs);
    }

    public async Task<ServiceResult<TagPriceListDto>> Handle(GetTagPriceListQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var key = (request.IdOrSlug ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(key))
            return ServiceResult<TagPriceListDto>.Failed("Tag not found");

        Tag? tag = int.TryParse(key, out var tagId)
            ? await _repo.Query(t => t.Id == tagId && !t.IsDeleted)
                         .FirstOrDefaultAsync(cancellationToken)
            : await _repo.Query(t => (t.Slug == key || t.Name == key) && !t.IsDeleted)
                         .FirstOrDefaultAsync(cancellationToken);

        if (tag == null || !tag.IsActive)
            return ServiceResult<TagPriceListDto>.Failed("Tag not found");

        // The tag lives on ProductOffers, so only offers carrying it may price the family.
        var products = await _productRepository
            .Query(p => p.IsActive && !p.IsDeleted &&
                   p.ProductOffers.Any(po => po.IsActive && !po.IsDeleted &&
                        po.ProductOfferTags.Any(pot => !pot.IsDeleted && pot.TagId == tag.Id)))
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Specifications)
            .Include(p => p.ProductOffers).ThenInclude(po => po.ProductOfferTags)
            .Include(p => p.ProductOffers).ThenInclude(po => po.Discounts).ThenInclude(d => d.Discount)
            .ToListAsync(cancellationToken);

        var items = new List<TagPriceListItemDto>();

        foreach (var product in products)
        {
            var taggedOffers = product.ProductOffers
                .Where(po => po.IsActive && !po.IsDeleted &&
                       po.ProductOfferTags.Any(pot => !pot.IsDeleted && pot.TagId == tag.Id))
                .ToList();

            if (taggedOffers.Count == 0)
                continue;

            var bestOffer = taggedOffers
                    .Where(po => po.Inventory > 0)
                    .OrderBy(po => po.GetFinalPrice(now))
                    .FirstOrDefault()
                ?? taggedOffers
                    .OrderBy(po => po.GetFinalPrice(now))
                    .FirstOrDefault();

            if (bestOffer == null)
                continue;

            var activeDiscount = bestOffer.Discounts
                .Where(d => d.Discount != null && !d.IsDeleted && d.IsActive)
                .Select(d => d.Discount)
                .Where(d => !d.IsDeleted && d.IsActive && d.StartDate <= now && d.EndDate >= now)
                .OrderByDescending(d => d.Priority)
                .FirstOrDefault();

            var basePrice = bestOffer.BasePrice;
            var finalPrice = bestOffer.GetFinalPrice(now);
            var inventory = taggedOffers.Sum(po => po.Inventory);

            items.Add(new TagPriceListItemDto
            {
                ProductId = product.Id,
                Slug = product.Slug,
                Name = product.Name,
                Description = product.Description,
                MainImage = product.Images
                    .Where(i => !i.IsDeleted && i.IsMain)
                    .Select(i => i.ImageUrl.TrimStart('/'))
                    .FirstOrDefault()
                    ?? product.Images
                        .Where(i => !i.IsDeleted)
                        .Select(i => i.ImageUrl.TrimStart('/'))
                        .FirstOrDefault(),
                CategoryName = product.Category?.PersianName,
                CategorySlug = product.Category?.Slug,
                BrandName = product.Brand?.Name,
                BrandSlug = product.Brand?.Slug,
                Code = $"RSK-{product.Id:0000}",
                Price = basePrice,
                FinalPrice = finalPrice,
                DiscountAmount = activeDiscount?.Amount,
                DiscountIsPercent = activeDiscount?.IsPercent,
                HasDiscount = finalPrice < basePrice,
                Inventory = inventory,
                InStock = inventory > 0,
                Diameter = SpecValue(product, VesselCatalogSpecs.DiameterKey),
                Height = SpecValue(product, VesselCatalogSpecs.HeightKey),
                PieceCount = SpecValue(product, VesselCatalogSpecs.PieceCountKey),
                Dimensions = product.Dimensions == null ? null : new ProductDimensionsDto
                {
                    Width = product.Dimensions.Width,
                    Height = product.Dimensions.Height,
                    Depth = product.Dimensions.Depth,
                    Weight = product.Dimensions.Weight,
                },
            });
        }

        var ordered = items
            .OrderBy(i => i.CategoryName)
            .ThenBy(i => i.Name)
            .ToList();

        var dto = new TagPriceListDto
        {
            TagId = tag.Id,
            TagName = tag.Name,
            TagSlug = tag.Slug,
            Currency = "IRR",
            ItemCount = ordered.Count,
            UpdatedAt = products.Count == 0
                ? (tag.UpdatedAt ?? tag.CreatedAt)
                : products.Max(p => p.UpdatedAt ?? p.CreatedAt),
            Items = ordered,
        };

        return ServiceResult<TagPriceListDto>.Ok(dto);
    }

    public async Task<ServiceResult<IEnumerable<TagFamilyDto>>> Handle(GetTagFamiliesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _productOfferRepository
            .Query(pot => !pot.IsDeleted &&
                   pot.ProductOffer.IsActive && !pot.ProductOffer.IsDeleted &&
                   pot.ProductOffer.Product.IsActive && !pot.ProductOffer.Product.IsDeleted &&
                   pot.Tag.IsActive && !pot.Tag.IsDeleted)
            .Select(pot => new
            {
                pot.TagId,
                TagName = pot.Tag.Name,
                TagSlug = pot.Tag.Slug,
                pot.ProductOffer.ProductId,
                CoverImage = pot.ProductOffer.Product.Images
                    .Where(i => !i.IsDeleted && i.IsMain)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var families = rows
            .GroupBy(r => r.TagId)
            .Select(g => new TagFamilyDto
            {
                Id = g.Key,
                Name = g.First().TagName ?? string.Empty,
                Slug = g.First().TagSlug ?? string.Empty,
                ProductCount = g.Select(r => r.ProductId).Distinct().Count(),
                CoverImage = g.Select(r => r.CoverImage)
                              .FirstOrDefault(i => !string.IsNullOrWhiteSpace(i))
                              ?.TrimStart('/'),
            })
            .OrderByDescending(f => f.ProductCount)
            .ThenBy(f => f.Name)
            .ToList();

        return ServiceResult<IEnumerable<TagFamilyDto>>.Ok(families);
    }

    private static string? SpecValue(Product product, string key) =>
        product.Specifications
            .Where(s => !s.IsDeleted && s.Key == key)
            .Select(s => s.Value)
            .FirstOrDefault();
}