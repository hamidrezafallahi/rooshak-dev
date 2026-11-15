using System.Globalization;
using System.Text.RegularExpressions;
using Application.Dtos;
using OnlineShop.Domain.Entities;

namespace Application.Common
{
    /// <summary>
    /// پیشنهادهای فعال یک محصول را به فهرست تنوع‌ها (مثلاً بزرگنمایی 2.5x / 3x / 3.5x) تبدیل می‌کند.
    /// اگر چند فروشنده یک تنوع را عرضه کنند، ارزان‌ترین پیشنهاد موجود آن تنوع انتخاب می‌شود.
    /// </summary>
    public static class ProductVariantMapper
    {
        private static readonly Regex LeadingNumber = new(@"\d+(?:[.,/]\d+)?", RegexOptions.Compiled);

        public static List<ProductVariantDto> Build(IEnumerable<ProductOffers> offers, DateTime now)
        {
            return offers
                .Where(o => o.IsActive && !o.IsDeleted && o.HasVariant)
                .GroupBy(o => o.VariantValue!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var best = g.Where(o => o.Inventory > 0).OrderBy(o => o.GetFinalPrice(now)).FirstOrDefault()
                               ?? g.OrderBy(o => o.GetFinalPrice(now)).First();
                    var discount = ActiveDiscount(best, now);
                    return new ProductVariantDto
                    {
                        OfferId = best.Id,
                        Name = best.VariantName,
                        Value = g.Key,
                        Price = best.BasePrice,
                        FinalPrice = best.GetFinalPrice(now),
                        DiscountIsPercent = discount?.IsPercent,
                        DiscountAmount = discount?.Amount,
                        Inventory = g.Sum(o => o.Inventory),
                        InStock = g.Any(o => o.Inventory > 0),
                    };
                })
                .OrderBy(v => SortKey(v.Value))
                .ThenBy(v => v.FinalPrice)
                .ToList();
        }

        private static Discount? ActiveDiscount(ProductOffers offer, DateTime now) =>
            offer.Discounts
                .Where(d => d.Discount != null && !d.IsDeleted && d.IsActive)
                .Select(d => d.Discount)
                .Where(d => !d.IsDeleted && d.IsActive && d.StartDate <= now && d.EndDate >= now)
                .OrderByDescending(d => d.Priority)
                .FirstOrDefault();

        /// <summary>«2.5x» قبل از «3x» و «3.5x»؛ مقدار غیرعددی به انتها می‌رود.</summary>
        private static decimal SortKey(string value)
        {
            var normalized = value
                .Replace('٫', '.')
                .Select(c => c is >= '۰' and <= '۹' ? (char)('0' + (c - '۰')) : c is >= '٠' and <= '٩' ? (char)('0' + (c - '٠')) : c)
                .ToArray();
            var match = LeadingNumber.Match(new string(normalized));
            return match.Success && decimal.TryParse(match.Value.Replace(',', '.').Split('/')[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var n)
                ? n
                : decimal.MaxValue;
        }
    }
}
