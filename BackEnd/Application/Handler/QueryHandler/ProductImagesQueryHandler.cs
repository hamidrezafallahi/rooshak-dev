using Application.Dtos;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
public class ProductImagesQueryHandler(
        IProductImageRepository _repo,
         IEntityConfigRepository _configRepo,
        IHttpContextAccessor _accessor) : 
    IRequestHandler<GetAllProductImagesQuery, ServiceResult<ListDto<GetProductImageDto>>>,
    IRequestHandler<GetMainImageByProductIdQuery, ServiceResult<GetProductImageDto>>,
    IRequestHandler<GetProductImagesByIdQuery, ServiceResult<GetProductImageDto>>,
    IRequestHandler<GetProductImagesByProductIdQuery, ServiceResult<List<GetProductImageDto>>>
{
    public async Task<ServiceResult<List<GetProductImageDto>>> Handle(GetProductImagesByProductIdQuery request, CancellationToken cancellationToken)
    {
        var images = await _repo.Query(p => p.ProductId == request.ProductId)
            .OrderByDescending(p => p.IsMain)
            .ThenBy(p => p.Id)
            .Select(p => new GetProductImageDto
            {
                Id = p.Id,
                IsMain = p.IsMain,
                IsActive = p.IsActive,
                ProductId = p.ProductId,
                ProductName = p.Product.Name,
                ProductImageUrl = p.ImageUrl,
                OriginalImageUrl = p.OriginalUrl
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<List<GetProductImageDto>>.Ok(images);
    }

    public async Task<ServiceResult<ListDto<GetProductImageDto>>> Handle( GetAllProductImagesQuery request,  CancellationToken cancellationToken)
    {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<ProductImage> query;
        if (request.Q is not null && request.Q.Length > 0)
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repo.Query().Include(i => i.Product).Where(i => i.Product.Name.Contains(request.Q));
            }
            else
            {
                query = _repo.Query(b => b.IsActive).Include(i => i.Product).Where(i => i.Product.Name.Contains(request.Q));
            }

             
        }
        else
        {
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {

                query = _repo.Query().Include(i => i.Product);
            }
            else
            {
                query = _repo.Query(b => b.IsActive).Include(i => i.Product);
            }
        }

       


        int totalCount = await query.CountAsync(cancellationToken);
        // Newest first, so the admin "recent images" list shows the latest uploads on top.
        var pagedproductImages = await query
            .OrderByDescending(i => i.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var productImagesDto = pagedproductImages.Select(p => new GetProductImageDto
        {
            Id = p.Id,
            IsMain = p.IsMain,
            ProductId = p.ProductId,
            ProductName = p.Product.Name,
            IsActive = p.IsActive,
            ProductImageUrl = p.ImageUrl,
            OriginalImageUrl = p.OriginalUrl,
        }).ToList();

        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("productImages");
        }

        var resultDto = new ListDto<GetProductImageDto>
        {
            Records = productImagesDto,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<GetProductImageDto>>.Ok(resultDto);

     
    }
    public async Task<ServiceResult<GetProductImageDto>> Handle( GetMainImageByProductIdQuery request, CancellationToken cancellationToken)
    {
        var productMainImage = _repo.Query(p => !p.IsDeleted && p.IsActive && p.ProductId == request.ProductId && p.IsMain)
            .Select(p => new GetProductImageDto
            {
                Id = p.Id,
                IsMain = p.IsMain,
                ProductId = p.ProductId,
                ProductImageUrl = p.ImageUrl
            }).FirstOrDefault();


        return ServiceResult<GetProductImageDto>.Ok(productMainImage);
    }
    public async Task<ServiceResult<GetProductImageDto>> Handle(GetProductImagesByIdQuery request, CancellationToken cancellationToken)
    {
        var productMainImage = _repo.Query(p => !p.IsDeleted && p.IsActive && p.Id == request.Id)
            .Select(p => new GetProductImageDto
            {
                Id = p.Id,
                IsMain = p.IsMain,
                ProductId = p.ProductId,
                ProductImageUrl = p.ImageUrl
            }).FirstOrDefault();
        return ServiceResult<GetProductImageDto>.Ok(productMainImage);
    }


}
 