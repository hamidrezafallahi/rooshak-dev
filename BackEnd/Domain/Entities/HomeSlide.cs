using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// اسلاید اسلایدر میانی صفحه‌ی اصلی (جدول HomeSlides). کاملاً جدا از بنر هیرو (<see cref="Slide"/>):
    /// جدول، API و صفحه‌ی ادمین خودش را دارد و هیچ عکسی بین این دو مشترک نیست.
    /// </summary>
    public class HomeSlide : BaseEntity
    {
        public const int MaxTitleLength = 150;
        public const int MaxUrlLength = 300;

        private HomeSlide() { }

        public string ImageUrl { get; private set; } = string.Empty;
        /// <summary>عکس اختیاری مخصوص موبایل؛ خالی = همان عکس دسکتاپ.</summary>
        public string MobileImageUrl { get; private set; } = string.Empty;
        /// <summary>مقصد کلیک (داخلی مثل products یا کامل)؛ خالی = بدون لینک.</summary>
        public string LinkUrl { get; private set; } = string.Empty;
        /// <summary>عنوان اختیاری؛ متن جایگزین عکس و برچسب دکمه‌ی اسلاید.</summary>
        public string TitleFa { get; private set; } = string.Empty;
        public string TitleEn { get; private set; } = string.Empty;
        /// <summary>ترتیب نمایش؛ عدد کوچک‌تر زودتر.</summary>
        public int DisplayOrder { get; private set; }

        public static HomeSlide Create(string? linkUrl, string? titleFa, string? titleEn, int? displayOrder, int currentUserId)
        {
            var slide = new HomeSlide();
            slide.Apply(linkUrl, titleFa, titleEn, displayOrder);
            slide.MarkCreated(currentUserId);
            return slide;
        }

        /// <summary>null یعنی «بدون تغییر»؛ رشته‌ی خالی مقدار اختیاری را پاک می‌کند.</summary>
        public void Update(string? linkUrl, string? titleFa, string? titleEn, int? displayOrder, int currentUserId)
        {
            Apply(linkUrl ?? LinkUrl, titleFa ?? TitleFa, titleEn ?? TitleEn, displayOrder ?? DisplayOrder);
            MarkUpdated(currentUserId);
        }

        public void SetImage(string? imageUrl, int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(imageUrl)) ImageUrl = imageUrl;
            MarkUpdated(currentUserId);
        }

        public void SetMobileImage(string? imageUrl, int currentUserId)
        {
            MobileImageUrl = imageUrl?.Trim() ?? string.Empty;
            MarkUpdated(currentUserId);
        }

        private void Apply(string? linkUrl, string? titleFa, string? titleEn, int? displayOrder)
        {
            LinkUrl = Limit(linkUrl, MaxUrlLength, "آدرس لینک");
            TitleFa = Limit(titleFa, MaxTitleLength, "عنوان فارسی");
            TitleEn = Limit(titleEn, MaxTitleLength, "عنوان انگلیسی");
            DisplayOrder = displayOrder ?? 0;
        }

        private static string Limit(string? value, int max, string field)
        {
            var clean = value?.Trim() ?? string.Empty;
            if (clean.Length > max)
                throw new DomainException($"{field} حداکثر {max} کاراکتر است");
            return clean;
        }
    }
}
