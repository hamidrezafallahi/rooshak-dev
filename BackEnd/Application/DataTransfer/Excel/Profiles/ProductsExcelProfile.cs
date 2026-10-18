using Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

namespace Application.DataTransfer.Excel.Profiles
{
    /// <summary>
    /// Products together with their dependent data in one workbook: the products sheet plus the
    /// sheets of <see cref="ProductOffersExcelProfile"/> (price/inventory) and
    /// <see cref="SpecificationsExcelProfile"/>. Child rows point at a product by Id, or by the
    /// product row's Ref when the product is new in the same file.
    /// Export filters: categoryId, brandId.
    /// </summary>
    public sealed class ProductsExcelProfile : ExcelEntityProfile
    {
        private const string ProductsSheet = "products";

        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;
        private readonly IBrandRepository _brands;
        private readonly ExcelEntityProfile[] _children;

        public ProductsExcelProfile(
            IMediator mediator,
            IProductRepository products,
            IProductOfferRepository offers,
            IProductSpecificationRepository specifications,
            ICategoryRepository categories,
            IBrandRepository brands) : base(mediator)
        {
            _products = products;
            _categories = categories;
            _brands = brands;
            _children =
            [
                new ProductOffersExcelProfile(mediator, offers, products, ProductsSheet),
                new SpecificationsExcelProfile(mediator, specifications, products, ProductsSheet),
            ];
        }

        public override string EntityKey => "products";
        public override string DisplayName => "محصولات";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet(ProductsSheet, "محصولات")
                .Describe("هر ردیف یک محصول. قیمت/موجودی و مشخصات در شیت‌های بعدی وارد می‌شوند.")
                .Id().Ref()
                .Text("Name", "نام محصول", required: true, width: 30)
                .Lookup("CategoryId", "دسته‌بندی", ExcelLookupKeys.Categories, required: true)
                .Lookup("BrandId", "برند", ExcelLookupKeys.Brands, required: true)
                .Text("Description", "توضیحات", width: 50)
                .Text("Slug", "نامک (Slug)", description: "خالی بماند تا از روی نام ساخته شود.")
                .Decimal("Width", "عرض")
                .Decimal("Height", "ارتفاع")
                .Decimal("Depth", "عمق")
                .Decimal("Weight", "وزن")
                .Text("SeoTitleFa", "عنوان سئو (فارسی)")
                .Text("SeoTitleEn", "عنوان سئو (انگلیسی)")
                .Text("MetaDescriptionFa", "توضیحات متا (فارسی)")
                .Text("MetaDescriptionEn", "توضیحات متا (انگلیسی)")
                .Boolean("IsActive", "فعال")
                .ReadOnly("MainImage", "تصویر اصلی")
                .Build();

            foreach (var child in _children)
                foreach (var sheet in child.Sheets)
                    yield return sheet;
        }

        public override async Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken ct) =>
        [
            await ExcelLookups.CategoriesAsync(_categories, ct),
            await ExcelLookups.BrandsAsync(_brands, ct),
            await ExcelLookups.ProductsAsync(_products, ct),
        ];

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = _products.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(p => p.Name.Contains(request.Q) || p.Slug.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(p => request.Ids.Contains(p.Id));
            if (request.FilterInt("categoryId") is { } categoryId) query = query.Where(p => p.CategoryId == categoryId);
            if (request.FilterInt("brandId") is { } brandId) query = query.Where(p => p.BrandId == brandId);

            var products = await query
                .OrderBy(p => p.Id)
                .Select(p => new
                {
                    p.Id, p.Name, p.CategoryId, p.BrandId, p.Description, p.Slug,
                    p.Dimensions, p.SeoTitleFa, p.SeoTitleEn, p.MetaDescriptionFa, p.MetaDescriptionEn, p.IsActive,
                    MainImage = p.Images.Where(i => i.IsMain && !i.IsDeleted).Select(i => i.ImageUrl).FirstOrDefault(),
                })
                .ToListAsync(ct);

            var result = new Dictionary<string, List<ExcelRow>>
            {
                [ProductsSheet] = products.Select(p => Row(new Dictionary<string, object?>
                {
                    ["Id"] = p.Id,
                    ["Name"] = p.Name,
                    ["CategoryId"] = p.CategoryId,
                    ["BrandId"] = p.BrandId,
                    ["Description"] = p.Description,
                    ["Slug"] = p.Slug,
                    ["Width"] = p.Dimensions?.Width,
                    ["Height"] = p.Dimensions?.Height,
                    ["Depth"] = p.Dimensions?.Depth,
                    ["Weight"] = p.Dimensions?.Weight,
                    ["SeoTitleFa"] = p.SeoTitleFa,
                    ["SeoTitleEn"] = p.SeoTitleEn,
                    ["MetaDescriptionFa"] = p.MetaDescriptionFa,
                    ["MetaDescriptionEn"] = p.MetaDescriptionEn,
                    ["IsActive"] = p.IsActive,
                    ["MainImage"] = p.MainImage,
                })).ToList(),
            };

            // Child sheets contain exactly the rows of the exported products.
            var childRequest = new ExcelExportRequest
            {
                Filters = new() { ["productIds"] = string.Join(',', products.Select(p => p.Id)) },
            };
            foreach (var child in _children.Where(c => c.Sheets.Any(s => request.IncludesSheet(s.Key))))
            {
                var rows = products.Count == 0
                    ? child.Sheets.ToDictionary(s => s.Key, _ => new List<ExcelRow>())
                    : await child.ExportAsync(childRequest, ct);
                foreach (var (sheet, sheetRows) in rows)
                    result[sheet] = sheetRows;
            }

            return result;
        }

        public override async Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;

            // Parents first so the child sheets can resolve the Refs of new products.
            await context.ProcessSheetAsync(ProductsSheet, async row =>
            {
                var categoryId = context.ResolveId(MainSheet, row, "CategoryId");
                var brandId = context.ResolveId(MainSheet, row, "BrandId");

                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateProductCommand
                    {
                        Name = row.RequireString("Name"),
                        Description = row.GetString("Description") ?? string.Empty,
                        CategoryId = categoryId!.Value,
                        BrandId = brandId!.Value,
                        Slug = row.GetString("Slug"),
                        Width = row.GetDecimal("Width") ?? 0,
                        Height = row.GetDecimal("Height") ?? 0,
                        Depth = row.GetDecimal("Depth") ?? 0,
                        Weight = row.GetDecimal("Weight") ?? 0,
                        SeoTitleFa = row.GetString("SeoTitleFa"),
                        SeoTitleEn = row.GetString("SeoTitleEn"),
                        MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                        MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                    }, ct),
                    update: async id =>
                    {
                        var current = await _products.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"محصولی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateProductCommand
                        {
                            Id = id,
                            Name = row.GetString("Name"),
                            Description = row.GetString("Description"),
                            CategoryId = categoryId,
                            BrandId = brandId,
                            Slug = row.GetString("Slug"),
                            // The command defaults dimensions to 0, so blank cells must carry the current value.
                            Width = row.GetDecimal("Width") ?? current.Dimensions?.Width,
                            Height = row.GetDecimal("Height") ?? current.Dimensions?.Height,
                            Depth = row.GetDecimal("Depth") ?? current.Dimensions?.Depth,
                            Weight = row.GetDecimal("Weight") ?? current.Dimensions?.Weight,
                            SeoTitleFa = row.GetString("SeoTitleFa"),
                            SeoTitleEn = row.GetString("SeoTitleEn"),
                            MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                            MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                        }, ct);
                    });

                await SetActiveAsync<ActiveProductCommand>(row, result.Id!.Value, ct);
                return result;
            });

            foreach (var child in _children)
                await child.ImportAsync(context);
        }
    }
}
