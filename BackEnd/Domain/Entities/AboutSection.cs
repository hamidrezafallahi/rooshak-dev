using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// یک بخش از صفحه‌ی «درباره ما / داستان ما» (جدول AboutSections) با متن دوزبانه.
    /// هر بخش کلید ثابتی دارد (مثل hero یا chapter-optics) که جای آن را در صفحه تعیین می‌کند؛
    /// ادمین فقط تیترها و متن‌ها را ویرایش یا بخش را خاموش می‌کند.
    /// </summary>
    public class AboutSection : BaseEntity
    {
        public const int MaxKeyLength = 50;
        public const int MaxShortLength = 200;
        public const int MaxBodyLength = 4000;

        private AboutSection() { }

        public string SectionKey { get; private set; } = string.Empty;
        public int DisplayOrder { get; private set; }

        public string EyebrowFa { get; private set; } = string.Empty;
        public string EyebrowEn { get; private set; } = string.Empty;
        public string TitleFa { get; private set; } = string.Empty;
        public string TitleEn { get; private set; } = string.Empty;
        public string BodyFa { get; private set; } = string.Empty;
        public string BodyEn { get; private set; } = string.Empty;
        /// <summary>متن لینک اختیاری بخش (مثلاً «رزرو وقت»).</summary>
        public string LinkTextFa { get; private set; } = string.Empty;
        public string LinkTextEn { get; private set; } = string.Empty;
        /// <summary>آدرس داخلی لینک (مثل cooperation)؛ خالی = آدرس پیش‌فرض همان بخش.</summary>
        public string LinkUrl { get; private set; } = string.Empty;

        public static AboutSection Create(string sectionKey, int? displayOrder, AboutSectionText text, int currentUserId)
        {
            var key = sectionKey?.Trim().ToLowerInvariant() ?? string.Empty;
            if (key.Length == 0)
                throw new DomainException("کلید بخش الزامی است");
            if (key.Length > MaxKeyLength)
                throw new DomainException($"کلید بخش حداکثر {MaxKeyLength} کاراکتر است");

            var section = new AboutSection { SectionKey = key };
            section.Apply(text, displayOrder ?? 0);
            section.MarkCreated(currentUserId);
            return section;
        }

        /// <summary>null یعنی «بدون تغییر»؛ رشته‌ی خالی متن را پاک می‌کند (و آن قسمت در سایت نمایش داده نمی‌شود).</summary>
        public void Update(AboutSectionText text, int? displayOrder, int currentUserId)
        {
            var merged = new AboutSectionText(
                text.EyebrowFa ?? EyebrowFa, text.EyebrowEn ?? EyebrowEn,
                text.TitleFa ?? TitleFa, text.TitleEn ?? TitleEn,
                text.BodyFa ?? BodyFa, text.BodyEn ?? BodyEn,
                text.LinkTextFa ?? LinkTextFa, text.LinkTextEn ?? LinkTextEn,
                text.LinkUrl ?? LinkUrl);
            Apply(merged, displayOrder ?? DisplayOrder);
            MarkUpdated(currentUserId);
        }

        private void Apply(AboutSectionText t, int displayOrder)
        {
            EyebrowFa = Limit(t.EyebrowFa, MaxShortLength, "برچسب فارسی");
            EyebrowEn = Limit(t.EyebrowEn, MaxShortLength, "برچسب انگلیسی");
            TitleFa = Limit(t.TitleFa, MaxShortLength, "تیتر فارسی");
            TitleEn = Limit(t.TitleEn, MaxShortLength, "تیتر انگلیسی");
            BodyFa = Limit(t.BodyFa, MaxBodyLength, "متن فارسی");
            BodyEn = Limit(t.BodyEn, MaxBodyLength, "متن انگلیسی");
            LinkTextFa = Limit(t.LinkTextFa, MaxShortLength, "متن لینک فارسی");
            LinkTextEn = Limit(t.LinkTextEn, MaxShortLength, "متن لینک انگلیسی");
            LinkUrl = Limit(t.LinkUrl, MaxShortLength, "آدرس لینک");
            DisplayOrder = displayOrder;
        }

        private static string Limit(string? value, int max, string field)
        {
            var clean = value?.Trim() ?? string.Empty;
            if (clean.Length > max)
                throw new DomainException($"{field} حداکثر {max} کاراکتر است");
            return clean;
        }
    }

    /// <summary>متن‌های دوزبانه‌ی یک بخش درباره ما (value object برای جلوگیری از لیست طولانی پارامتر).</summary>
    public sealed record AboutSectionText(
        string? EyebrowFa,
        string? EyebrowEn,
        string? TitleFa,
        string? TitleEn,
        string? BodyFa,
        string? BodyEn,
        string? LinkTextFa,
        string? LinkTextEn,
        string? LinkUrl);
}
