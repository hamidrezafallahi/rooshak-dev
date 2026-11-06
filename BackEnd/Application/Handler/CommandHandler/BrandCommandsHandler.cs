using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using Services.Services.Uploader.DTO;

namespace Application.Handler.CommandHandler
{
    public class BrandCommandHandler(
        IBrandRepository _repo,
        IHttpContextAccessor _accessor,
        IUploaderService _uploaderService) :
        IRequestHandler<CreateBrandCommand, ServiceResult<IdDto>>,
        IRequestHandler<UpdateBrandCommand, ServiceResult<IdDto>>,
        IRequestHandler<DeleteBrandCommand, ServiceResult<IdDto>>,
        IRequestHandler<ActiveBrandCommand, ServiceResult<IdDto>>
    {
        public async Task<ServiceResult<IdDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var brand = Brand.Create(
                name: request.Name,
                description: request.Description,
                currentUserId: userId.Value,
                slug: request.Slug
            );
            brand.SetSeo(
                request.SeoTitleFa,
                request.SeoTitleEn,
                request.MetaDescriptionFa,
                request.MetaDescriptionEn,
                userId.Value);
            await _repo.AddAsync(brand);
            await _repo.SaveChangesAsync(cancellationToken);
            brand.EnsureSlug(brand.Id);
            await _repo.SaveChangesAsync(cancellationToken);

            if (request.LogoFile is not null)
            {
                var uploadDto = new UploadDTO
                {
                    File = request.LogoFile,
                    Path = UploadPaths.Brands(brand.Id)
                };
                var logoUrl = await _uploaderService.UploadAsWebp(uploadDto);
                if (UploadPaths.IsStoredPath(logoUrl))
                    brand.Update(userId.Value, null, logoUrl, null, false);
            }

            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = brand.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var brand = await _repo.GetByIdAsync(request.Id);
            if (brand == null)
                return ServiceResult<IdDto>.Failed("برند مورد نظر پیدا نشد");

            string? logoUrl = null;
            if (request.LogoFile is not null)
            {
                await _uploaderService.DeleteStoredFile(
                    brand.LogoUrl,
                    UploadPaths.Brands(brand.Id));

                var uploadDto = new UploadDTO
                {
                    File = request.LogoFile,
                    Path = UploadPaths.Brands(brand.Id)
                };
                logoUrl = await _uploaderService.UploadAsWebp(uploadDto);
                if (!UploadPaths.IsStoredPath(logoUrl))
                    return ServiceResult<IdDto>.Failed("آپلود لوگو ناموفق بود");
            }

            brand.Update(
                currentUserId: userId.Value,
                name: request.Name ?? brand.Name,
                logoUrl: logoUrl ?? brand.LogoUrl,
                description: request.Description ?? brand.Description,
                isActive: request.IsActive ?? brand.IsActive,
                slug: request.Slug,
                seoTitleFa: request.SeoTitleFa,
                seoTitleEn: request.SeoTitleEn,
                metaDescriptionFa: request.MetaDescriptionFa,
                metaDescriptionEn: request.MetaDescriptionEn
            );
            brand.EnsureSlug(brand.Id);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = brand.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var brand = await _repo.GetByIdAsync(request.Id);
            if (brand == null)
                return ServiceResult<IdDto>.Failed("برند مورد نظر پیدا نشد");

            await _uploaderService.DeleteStoredFile(
                brand.LogoUrl,
                UploadPaths.Brands(brand.Id));

            brand.Delete(userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = brand.Id });
        }

        public async Task<ServiceResult<IdDto>> Handle(ActiveBrandCommand request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Unauthorized();

            var brand = await _repo.GetByIdAsync(request.Id);
            if (brand == null)
                return ServiceResult<IdDto>.Failed("برند مورد نظر پیدا نشد");

            brand.SetActive(request.IsActive, userId.Value);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResult<IdDto>.Ok(new IdDto { Id = brand.Id });
        }
    }
}
