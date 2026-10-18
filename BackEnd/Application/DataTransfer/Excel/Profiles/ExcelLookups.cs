using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Interfaces;

namespace Application.DataTransfer.Excel.Profiles
{
    public static class ExcelLookupKeys
    {
        public const string Categories = "categories";
        public const string Brands = "brands";
        public const string Products = "products";
        public const string ProductOffers = "productOffers";
        public const string Discounts = "discounts";
    }

    /// <summary>Shared lookup lists so every profile shows the same labels for the same entity.</summary>
    public static class ExcelLookups
    {
        public static async Task<ExcelLookup> CategoriesAsync(ICategoryRepository repository, CancellationToken ct) => new()
        {
            Key = ExcelLookupKeys.Categories,
            Caption = "دسته‌بندی‌ها",
            Items = await repository.Query().AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new ExcelLookupItem(c.Id, c.PersianName))
                .ToListAsync(ct),
        };

        public static async Task<ExcelLookup> BrandsAsync(IBrandRepository repository, CancellationToken ct) => new()
        {
            Key = ExcelLookupKeys.Brands,
            Caption = "برندها",
            Items = await repository.Query().AsNoTracking()
                .OrderBy(b => b.Id)
                .Select(b => new ExcelLookupItem(b.Id, b.Name))
                .ToListAsync(ct),
        };

        public static async Task<ExcelLookup> ProductsAsync(IProductRepository repository, CancellationToken ct) => new()
        {
            Key = ExcelLookupKeys.Products,
            Caption = "محصولات",
            Items = await repository.Query().AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new ExcelLookupItem(p.Id, p.Name))
                .ToListAsync(ct),
        };

        public static async Task<ExcelLookup> ProductOffersAsync(IProductOfferRepository repository, CancellationToken ct) => new()
        {
            Key = ExcelLookupKeys.ProductOffers,
            Caption = "پیشنهادهای فروش",
            Items = (await repository.Query().AsNoTracking()
                    .OrderBy(o => o.Id)
                    .Select(o => new { o.Id, o.Product.Name, o.BasePrice })
                    .ToListAsync(ct))
                .Select(o => new ExcelLookupItem(o.Id, $"{o.Name} ({o.BasePrice:#,0} ریال)"))
                .ToList(),
        };

        public static async Task<ExcelLookup> DiscountsAsync(IDiscountRepository repository, CancellationToken ct) => new()
        {
            Key = ExcelLookupKeys.Discounts,
            Caption = "تخفیف‌ها",
            Items = await repository.Query().AsNoTracking()
                .OrderBy(d => d.Id)
                .Select(d => new ExcelLookupItem(d.Id, d.Title))
                .ToListAsync(ct),
        };
    }
}
