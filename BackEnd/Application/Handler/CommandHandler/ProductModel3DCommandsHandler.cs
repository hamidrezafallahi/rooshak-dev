using Application.Commands;
using Application.Common;
using Application.Common.Interfaces;
using Application.Dtos;
using Application.Queries;
using Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.CommandHandler
{
    public class ProductModel3DCommandsHandler(
        IProductModel3DRepository _models,
        IProductScanSourceRepository _scans,
        IProductRepository _products,
        IModelFileStorage _storage,
        IUploaderService _uploader,
        IHttpContextAccessor _accessor) :
        IRequestHandler<UpsertProductModel3DCommand, ServiceResult<ProductModel3DAdminDto>>,
        IRequestHandler<RescaleProductModel3DCommand, ServiceResult<ProductModel3DAdminDto>>,
        IRequestHandler<DeleteProductModel3DCommand, ServiceResult<IdDto>>,
        IRequestHandler<AddProductScanSourcesCommand, ServiceResult<ProductModel3DAdminDto>>,
        IRequestHandler<DeleteProductScanSourceCommand, ServiceResult<IdDto>>,
        IRequestHandler<GetProductModel3DAdminQuery, ServiceResult<ProductModel3DAdminDto>>
    {
        public async Task<ServiceResult<ProductModel3DAdminDto>> Handle(UpsertProductModel3DCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("Unauthorized");

            var product = await _products.GetByIdAsync(request.ProductId);
            if (product == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("محصول پیدا نشد");

            var existing = await _models.GetByProductIdAsync(product.Id);
            if (existing == null && request.Model == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("ابتدا فایل مدل (GLB) را انتخاب کنید.");

            var dir = UploadPaths.ProductModels(product.Id);
            string? oldModel = null, oldUsdz = null;

            StoredFileResult? model = null, usdz = null;
            if (request.Model != null)
            {
                model = await _storage.SaveModelAsync(request.Model, dir);
                if (!model.Ok)
                    return ServiceResult<ProductModel3DAdminDto>.Failed(model.Error ?? "آپلود مدل ناموفق بود.");
            }
            if (request.Usdz != null)
            {
                usdz = await _storage.SaveUsdzAsync(request.Usdz, dir);
                if (!usdz.Ok)
                {
                    if (model?.Ok == true) await _uploader.DeleteStoredFile(model.Path, dir);
                    return ServiceResult<ProductModel3DAdminDto>.Failed(usdz.Error ?? "آپلود USDZ ناموفق بود.");
                }
            }

            if (existing == null)
            {
                existing = ProductModel3D.Create(product.Id, model!.Path!, model.SizeBytes, userId.Value);
                await _models.AddAsync(existing);
            }
            else
            {
                if (model != null)
                {
                    oldModel = existing.ModelUrl;
                    existing.SetModel(model.Path!, model.SizeBytes, userId.Value);
                }
            }

            if (usdz != null)
            {
                oldUsdz = existing.UsdzUrl;
                existing.SetUsdz(usdz.Path, usdz.SizeBytes, userId.Value);
            }
            else if (request.RemoveUsdz && existing.UsdzUrl != null)
            {
                oldUsdz = existing.UsdzUrl;
                existing.SetUsdz(null, null, userId.Value);
            }

            await _models.SaveChangesAsync(ct);

            // Old files are removed only after the DB row points at the new ones.
            if (oldModel != null) await _uploader.DeleteStoredFile(oldModel, dir);
            if (oldUsdz != null) await _uploader.DeleteStoredFile(oldUsdz, dir);

            return ServiceResult<ProductModel3DAdminDto>.Ok(await BuildAdminDto(product.Id));
        }

        public async Task<ServiceResult<ProductModel3DAdminDto>> Handle(RescaleProductModel3DCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("Unauthorized");

            var existing = await _models.GetByProductIdAsync(request.ProductId);
            if (existing == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("مدل سه‌بعدی پیدا نشد");

            var dir = UploadPaths.ProductModels(request.ProductId);
            var scaled = await _storage.RescaleModelAsync(existing.ModelUrl, request.Factor, dir);
            if (!scaled.Ok)
                return ServiceResult<ProductModel3DAdminDto>.Failed(scaled.Error ?? "تغییر اندازه ممکن نشد.");

            var oldUrl = existing.ModelUrl;
            existing.SetModel(scaled.Path!, scaled.SizeBytes, userId.Value);
            await _models.SaveChangesAsync(ct);
            await _uploader.DeleteStoredFile(oldUrl, dir);

            return ServiceResult<ProductModel3DAdminDto>.Ok(await BuildAdminDto(request.ProductId));
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteProductModel3DCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var existing = await _models.GetByProductIdAsync(request.ProductId);
            if (existing == null)
                return ServiceResult<IdDto>.Failed("مدل سه‌بعدی پیدا نشد");

            var dir = UploadPaths.ProductModels(request.ProductId);
            var modelUrl = existing.ModelUrl;
            var usdzUrl = existing.UsdzUrl;

            existing.Delete(userId.Value);
            await _models.SaveChangesAsync(ct);

            await _uploader.DeleteStoredFile(modelUrl, dir);
            if (usdzUrl != null) await _uploader.DeleteStoredFile(usdzUrl, dir);

            return ServiceResult<IdDto>.Ok(new IdDto { Id = request.ProductId });
        }

        public async Task<ServiceResult<ProductModel3DAdminDto>> Handle(AddProductScanSourcesCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("Unauthorized");

            var product = await _products.GetByIdAsync(request.ProductId);
            if (product == null)
                return ServiceResult<ProductModel3DAdminDto>.Failed("محصول پیدا نشد");
            if (request.Files == null || request.Files.Count == 0)
                return ServiceResult<ProductModel3DAdminDto>.Failed("فایلی انتخاب نشده است.");

            var dir = UploadPaths.ProductScans(product.Id);
            foreach (var file in request.Files)
            {
                var stored = await _storage.SaveScanSourceAsync(file, dir);
                if (!stored.Ok)
                    return ServiceResult<ProductModel3DAdminDto>.Failed(stored.Error ?? "آپلود ناموفق بود.");

                await _scans.AddAsync(ProductScanSource.Create(
                    product.Id, stored.Path!, _storage.ScanKind(stored.Path!), stored.SizeBytes, userId.Value));
                // Persist per file so a later failure does not orphan earlier uploads on disk.
                await _scans.SaveChangesAsync(ct);
            }

            return ServiceResult<ProductModel3DAdminDto>.Ok(await BuildAdminDto(product.Id));
        }

        public async Task<ServiceResult<IdDto>> Handle(DeleteProductScanSourceCommand request, CancellationToken ct)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<IdDto>.Failed("Unauthorized");

            var source = await _scans.GetByIdAsync(request.Id);
            if (source == null)
                return ServiceResult<IdDto>.Failed("فایل پیدا نشد");

            var url = source.FileUrl;
            var productId = source.ProductId;
            source.Delete(userId.Value);
            await _scans.SaveChangesAsync(ct);
            await _uploader.DeleteStoredFile(url, UploadPaths.ProductScans(productId));

            return ServiceResult<IdDto>.Ok(new IdDto { Id = request.Id });
        }

        public async Task<ServiceResult<ProductModel3DAdminDto>> Handle(GetProductModel3DAdminQuery request, CancellationToken ct) =>
            ServiceResult<ProductModel3DAdminDto>.Ok(await BuildAdminDto(request.ProductId));

        private async Task<ProductModel3DAdminDto> BuildAdminDto(int productId)
        {
            var model = await _models.GetByProductIdAsync(productId);
            var scans = await _scans.GetByProductIdAsync(productId);
            return new ProductModel3DAdminDto
            {
                ProductId = productId,
                ModelId = model?.Id,
                ModelUrl = model?.ModelUrl,
                ModelSizeBytes = model?.ModelSizeBytes,
                UsdzUrl = model?.UsdzUrl,
                UsdzSizeBytes = model?.UsdzSizeBytes,
                ScanSources = scans.Select(s => new ProductScanSourceDto
                {
                    Id = s.Id,
                    FileUrl = s.FileUrl,
                    Kind = s.Kind,
                    SizeBytes = s.SizeBytes
                }).ToList()
            };
        }
    }
}
