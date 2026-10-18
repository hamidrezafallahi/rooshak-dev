using Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using OnlineShop.Domain.Interfaces;

namespace Application.DataTransfer.Excel.Profiles
{
    /// <summary>Brands. Logos are managed in the admin form (images cannot travel in Excel).</summary>
    public sealed class BrandsExcelProfile(IMediator mediator, IBrandRepository brands) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "brands";
        public override string DisplayName => "برندها";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("brands", "برندها")
                .Id()
                .Text("Name", "نام برند", required: true, width: 25)
                .Text("Description", "توضیحات", width: 50)
                .Text("Slug", "نامک (Slug)")
                .Text("SeoTitleFa", "عنوان سئو (فارسی)")
                .Text("SeoTitleEn", "عنوان سئو (انگلیسی)")
                .Text("MetaDescriptionFa", "توضیحات متا (فارسی)")
                .Text("MetaDescriptionEn", "توضیحات متا (انگلیسی)")
                .Boolean("IsActive", "فعال")
                .ReadOnly("LogoUrl", "لوگو")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = brands.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(b => b.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(b => b.Name.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(b => request.Ids.Contains(b.Id));

            var rows = await query.OrderBy(b => b.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["brands"] = rows.Select(b => Row(new Dictionary<string, object?>
                {
                    ["Id"] = b.Id,
                    ["Name"] = b.Name,
                    ["Description"] = b.Description,
                    ["Slug"] = b.Slug,
                    ["SeoTitleFa"] = b.SeoTitleFa,
                    ["SeoTitleEn"] = b.SeoTitleEn,
                    ["MetaDescriptionFa"] = b.MetaDescriptionFa,
                    ["MetaDescriptionEn"] = b.MetaDescriptionEn,
                    ["IsActive"] = b.IsActive,
                    ["LogoUrl"] = b.LogoUrl,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context) =>
            context.ProcessSheetAsync("brands", row => UpsertAsync(row,
                create: () => SendAsync(new CreateBrandCommand
                {
                    Name = row.RequireString("Name"),
                    Description = row.GetString("Description") ?? string.Empty,
                    Slug = row.GetString("Slug"),
                    SeoTitleFa = row.GetString("SeoTitleFa"),
                    SeoTitleEn = row.GetString("SeoTitleEn"),
                    MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                    MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                    IsActive = row.GetBool("IsActive"),
                }, context.CancellationToken),
                update: id => SendAsync(new UpdateBrandCommand
                {
                    Id = id,
                    Name = row.GetString("Name"),
                    Description = row.GetString("Description"),
                    Slug = row.GetString("Slug"),
                    SeoTitleFa = row.GetString("SeoTitleFa"),
                    SeoTitleEn = row.GetString("SeoTitleEn"),
                    MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                    MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                    IsActive = row.GetBool("IsActive"),
                }, context.CancellationToken)));
    }

    /// <summary>
    /// Categories (a tree). The parent column accepts an existing category or the Ref of a row
    /// in the same file; rows are processed parents-first whatever their order in the sheet.
    /// </summary>
    public sealed class CategoriesExcelProfile(IMediator mediator, ICategoryRepository categories) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "categories";
        public override string DisplayName => "دسته‌بندی‌ها";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("categories", "دسته‌بندی‌ها")
                .Describe("برای زیرمجموعه، دسته‌ی والد را انتخاب کنید یا «کلید ارجاع» ردیف والد در همین فایل را بنویسید. خالی = دسته‌ی اصلی (در ویرایش: بدون تغییر).")
                .Id().Ref()
                .Text("PersianName", "نام فارسی", required: true, width: 25)
                .Text("EnglishName", "نام انگلیسی", required: true, width: 25)
                .Lookup("ParentCategoryId", "دسته‌ی والد", ExcelLookupKeys.Categories, referencesSheet: "categories")
                .Text("CategoryPersianDesc", "توضیحات فارسی", width: 50)
                .Text("CategoryEnglishDesc", "توضیحات انگلیسی", width: 50)
                .Boolean("IsShowInLanding", "نمایش در صفحه اصلی")
                .Text("Slug", "نامک (Slug)")
                .Text("SeoTitleFa", "عنوان سئو (فارسی)")
                .Text("SeoTitleEn", "عنوان سئو (انگلیسی)")
                .Text("MetaDescriptionFa", "توضیحات متا (فارسی)")
                .Text("MetaDescriptionEn", "توضیحات متا (انگلیسی)")
                .Text("FaqJson", "سوالات متداول (JSON)")
                .Boolean("IsActive", "فعال")
                .ReadOnly("ImageUrl", "تصویر")
                .Build();
        }

        public override async Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken ct) =>
            [await ExcelLookups.CategoriesAsync(categories, ct)];

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = categories.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(c => c.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(c => c.PersianName.Contains(request.Q) || c.EnglishName.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(c => request.Ids.Contains(c.Id));
            if (request.FilterInt("parentCategoryId") is { } parentId) query = query.Where(c => c.ParentCategoryId == parentId);

            var rows = await query.OrderBy(c => c.ParentCategoryId ?? 0).ThenBy(c => c.Id).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["categories"] = rows.Select(c => Row(new Dictionary<string, object?>
                {
                    ["Id"] = c.Id,
                    ["PersianName"] = c.PersianName,
                    ["EnglishName"] = c.EnglishName,
                    ["ParentCategoryId"] = c.ParentCategoryId,
                    ["CategoryPersianDesc"] = c.CategoryPersianDesc,
                    ["CategoryEnglishDesc"] = c.CategoryEnglishDesc,
                    ["IsShowInLanding"] = c.IsShowInLanding,
                    ["Slug"] = c.Slug,
                    ["SeoTitleFa"] = c.SeoTitleFa,
                    ["SeoTitleEn"] = c.SeoTitleEn,
                    ["MetaDescriptionFa"] = c.MetaDescriptionFa,
                    ["MetaDescriptionEn"] = c.MetaDescriptionEn,
                    ["FaqJson"] = c.FaqJson,
                    ["IsActive"] = c.IsActive,
                    ["ImageUrl"] = c.ImageUrl,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var schema = MainSheet;
            var ct = context.CancellationToken;

            return context.ProcessSheetAsync("categories", async row =>
            {
                var parentId = context.ResolveId(schema, row, "ParentCategoryId");
                if (parentId is not null && parentId == row.Id)
                    throw new ExcelRowException("یک دسته نمی‌تواند والد خودش باشد.");

                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateCategoryCommand
                    {
                        PersianName = row.RequireString("PersianName"),
                        EnglishName = row.RequireString("EnglishName"),
                        ParentCategoryId = parentId,
                        CategoryPersianDesc = row.GetString("CategoryPersianDesc") ?? string.Empty,
                        CategoryEnglishDesc = row.GetString("CategoryEnglishDesc") ?? string.Empty,
                        IsShowInLanding = row.GetBool("IsShowInLanding") ?? false,
                        IsActive = row.GetBool("IsActive"),
                        Slug = row.GetString("Slug"),
                        SeoTitleFa = row.GetString("SeoTitleFa"),
                        SeoTitleEn = row.GetString("SeoTitleEn"),
                        MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                        MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                        FaqJson = row.GetString("FaqJson"),
                    }, ct),
                    update: async id =>
                    {
                        var current = await categories.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"دسته‌بندی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateCategoryCommand
                        {
                            Id = id,
                            PersianName = row.GetString("PersianName") ?? current.PersianName,
                            EnglishName = row.GetString("EnglishName") ?? current.EnglishName,
                            ParentCategoryId = parentId,
                            CategoryPersianDesc = row.GetString("CategoryPersianDesc") ?? current.CategoryPersianDesc,
                            CategoryEnglishDesc = row.GetString("CategoryEnglishDesc") ?? current.CategoryEnglishDesc,
                            IsShowInLanding = row.GetBool("IsShowInLanding") ?? current.IsShowInLanding,
                            Slug = row.GetString("Slug"),
                            SeoTitleFa = row.GetString("SeoTitleFa"),
                            SeoTitleEn = row.GetString("SeoTitleEn"),
                            MetaDescriptionFa = row.GetString("MetaDescriptionFa"),
                            MetaDescriptionEn = row.GetString("MetaDescriptionEn"),
                            FaqJson = row.GetString("FaqJson"),
                        }, ct);
                    });

                await SetActiveAsync<ActiveCategoryCommand>(row, result.Id!.Value, ct);
                return result;
            }, rows => ExcelImportContext.ParentsFirst(rows, "ParentCategoryId"));
        }
    }

    public sealed class TagsExcelProfile(IMediator mediator, ITagRepository tags) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "tags";
        public override string DisplayName => "برچسب‌ها";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("tags", "برچسب‌ها")
                .Id()
                .Text("Name", "نام برچسب", required: true, width: 25)
                .Text("Slug", "نامک (Slug)")
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = tags.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(t => t.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(t => t.Name.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(t => request.Ids.Contains(t.Id));

            var rows = await query.OrderBy(t => t.Id).Select(t => new { t.Id, t.Name, t.Slug, t.IsActive }).ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["tags"] = rows.Select(t => Row(new Dictionary<string, object?>
                {
                    ["Id"] = t.Id,
                    ["Name"] = t.Name,
                    ["Slug"] = t.Slug,
                    ["IsActive"] = t.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("tags", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateTagCommand { Name = row.RequireString("Name"), Slug = row.GetString("Slug") }, ct),
                    update: async id =>
                    {
                        var current = await tags.GetByIdAsync(id)
                                      ?? throw new ExcelRowException($"برچسبی با شناسه {id} پیدا نشد.");
                        return await SendAsync(new UpdateTagCommand { Id = id, Name = row.GetString("Name") ?? current.Name, Slug = row.GetString("Slug") }, ct);
                    });
                await SetActiveAsync<ActiveTagCommand>(row, result.Id!.Value, ct);
                return result;
            });
        }
    }

    /// <summary>Product offers (price/inventory) on their own; also available as a sheet of the products workbook.</summary>
    public sealed class ProductOffersExcelProfile(
        IMediator mediator,
        IProductOfferRepository offers,
        IProductRepository products,
        string? productsSheet = null) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "productOffers";
        public override string DisplayName => "قیمت و موجودی محصولات";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("productOffers", "قیمت و موجودی")
                .Describe("برای بروزرسانی گروهی قیمت/موجودی کافی است ستون‌های شناسه، قیمت پایه و موجودی را نگه دارید.")
                .Id()
                .Lookup("ProductId", "محصول", ExcelLookupKeys.Products, required: true, referencesSheet: productsSheet)
                .Decimal("BasePrice", "قیمت پایه (ریال)", required: true)
                .Integer("Inventory", "موجودی", required: true)
                .Boolean("IsActive", "فعال")
                .ReadOnly("SupplierName", "تأمین‌کننده")
                .Build();
        }

        public override async Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken ct) =>
            [await ExcelLookups.ProductsAsync(products, ct)];

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = offers.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(o => o.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q)) query = query.Where(o => o.Product.Name.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(o => request.Ids.Contains(o.Id));
            if (request.FilterInt("productId") is { } productId) query = query.Where(o => o.ProductId == productId);
            if (request.FilterIntList("productIds") is { } productIds) query = query.Where(o => productIds.Contains(o.ProductId));
            if (request.FilterInt("categoryId") is { } categoryId) query = query.Where(o => o.Product.CategoryId == categoryId);

            var rows = await query.OrderBy(o => o.ProductId).ThenBy(o => o.Id)
                .Select(o => new { o.Id, o.ProductId, o.BasePrice, o.Inventory, o.IsActive, SupplierName = o.Supplier.FullName })
                .ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["productOffers"] = rows.Select(o => Row(new Dictionary<string, object?>
                {
                    ["Id"] = o.Id,
                    ["ProductId"] = o.ProductId,
                    ["BasePrice"] = o.BasePrice,
                    ["Inventory"] = o.Inventory,
                    ["IsActive"] = o.IsActive,
                    ["SupplierName"] = o.SupplierName,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("productOffers", row => UpsertAsync(row,
                create: async () =>
                {
                    var id = await SendAsync(new CreateProductOfferCommand
                    {
                        ProductId = context.RequireId(MainSheet, row, "ProductId"),
                        BasePrice = row.RequireDecimal("BasePrice"),
                        Inventory = row.RequireInt("Inventory"),
                    }, ct);
                    await SetActiveAsync<ActiveProductOfferCommand>(row, id, ct);
                    return id;
                },
                update: id => SendAsync(new UpdateProductOfferCommand
                {
                    Id = id,
                    BasePrice = row.GetDecimal("BasePrice"),
                    Inventory = row.GetInt("Inventory"),
                    IsActive = row.GetBool("IsActive"),
                }, ct)));
        }
    }

    /// <summary>Product specifications on their own; also available as a sheet of the products workbook.</summary>
    public sealed class SpecificationsExcelProfile(
        IMediator mediator,
        IProductSpecificationRepository specifications,
        IProductRepository products,
        string? productsSheet = null) : ExcelEntityProfile(mediator)
    {
        public override string EntityKey => "specifications";
        public override string DisplayName => "مشخصات محصولات";

        protected override IEnumerable<ExcelSheetSchema> DefineSheets()
        {
            yield return Sheet("specifications", "مشخصات")
                .Id()
                .Lookup("ProductId", "محصول", ExcelLookupKeys.Products, required: true, referencesSheet: productsSheet)
                .Text("Key", "عنوان مشخصه", required: true)
                .Text("Value", "مقدار", required: true, width: 30)
                .Boolean("IsActive", "فعال")
                .Build();
        }

        public override async Task<IReadOnlyList<ExcelLookup>> GetLookupsAsync(CancellationToken ct) =>
            [await ExcelLookups.ProductsAsync(products, ct)];

        public override async Task<IReadOnlyDictionary<string, List<ExcelRow>>> ExportAsync(ExcelExportRequest request, CancellationToken ct)
        {
            var query = specifications.Query().AsNoTracking();
            if (request.OnlyActives == true) query = query.Where(s => s.IsActive);
            if (!string.IsNullOrWhiteSpace(request.Q))
                query = query.Where(s => s.Key.Contains(request.Q) || s.Value.Contains(request.Q) || s.Product.Name.Contains(request.Q));
            if (request.Ids is { Count: > 0 }) query = query.Where(s => request.Ids.Contains(s.Id));
            if (request.FilterInt("productId") is { } productId) query = query.Where(s => s.ProductId == productId);
            if (request.FilterIntList("productIds") is { } productIds) query = query.Where(s => productIds.Contains(s.ProductId));

            var rows = await query.OrderBy(s => s.ProductId).ThenBy(s => s.Id)
                .Select(s => new { s.Id, s.ProductId, s.Key, s.Value, s.IsActive })
                .ToListAsync(ct);
            return new Dictionary<string, List<ExcelRow>>
            {
                ["specifications"] = rows.Select(s => Row(new Dictionary<string, object?>
                {
                    ["Id"] = s.Id,
                    ["ProductId"] = s.ProductId,
                    ["Key"] = s.Key,
                    ["Value"] = s.Value,
                    ["IsActive"] = s.IsActive,
                })).ToList(),
            };
        }

        public override Task ImportAsync(ExcelImportContext context)
        {
            var ct = context.CancellationToken;
            return context.ProcessSheetAsync("specifications", async row =>
            {
                var result = await UpsertAsync(row,
                    create: () => SendAsync(new CreateProductSpecificationCommand
                    {
                        ProductId = context.RequireId(MainSheet, row, "ProductId"),
                        Key = row.RequireString("Key"),
                        Value = row.RequireString("Value"),
                    }, ct),
                    update: id => SendAsync(new UpdateProductSpecificationCommand
                    {
                        Id = id,
                        Key = row.GetString("Key")!,
                        Value = row.GetString("Value")!,
                    }, ct));

                if (row.GetBool("IsActive") is { } isActive)
                    await SendAsync(new ActiveProductSpecificationCommand { Id = result.Id!.Value, IsActive = isActive }, ct);
                return result;
            });
        }
    }
}
