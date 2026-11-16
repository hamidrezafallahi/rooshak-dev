using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// بنر هیروی صفحه‌ی اصلی (جدول Slides). اسلایدر پایین‌تر صفحه موجودیت جدای <see cref="HomeSlide"/> است.
    /// </summary>
    public class Slide : BaseEntity
    {
        public const int MaxLinkTextLength = 100;
        public const int MinTitleOpacity = 0;
        public const int MaxTitleOpacity = 100;

        private Slide() { }

        // ===== محتوا =====
        public string BannerUrl { get; private set; } = string.Empty;
        /// <summary>اختیاری؛ خالی یعنی روی هیرو نمایش داده نشود.</summary>
        public string BannerTitle { get; private set; } = string.Empty;
        /// <summary>اختیاری؛ خالی یعنی روی هیرو نمایش داده نشود.</summary>
        public string BannerDescription { get; private set; } = string.Empty;
        /// <summary>مقصد کلیک روی خود عکس / ویدیوی بنر.</summary>
        public string FirstUrl { get; private set; } = string.Empty;
        /// <summary>لینک دوم اختیاری؛ به شکل متن روی هیرو نمایش داده می‌شود.</summary>
        public string SecondUrl { get; private set; } = string.Empty;
        /// <summary>متن لینک دوم (فارسی/انگلیسی)؛ وقتی آدرس دوم داده شود هر دو الزامی‌اند.</summary>
        public string SecondUrlTextFa { get; private set; } = string.Empty;
        public string SecondUrlTextEn { get; private set; } = string.Empty;
        /// <summary>Optional hero video (mp4/webm). BannerUrl doubles as its poster / fallback image.</summary>
        public string VideoUrl { get; private set; } = string.Empty;
        /// <summary>نسخه‌ی موبایل: پوستر (عکس) و ویدیوی جدا برای صفحه‌های کوچک. خالی = از نسخه‌ی دسکتاپ استفاده می‌شود.</summary>
        public string MobileBannerUrl { get; private set; } = string.Empty;
        public string MobileVideoUrl { get; private set; } = string.Empty;
        public bool IsHero { get; private set; }
        /// <summary>
        /// شفافیت عنوان و توضیح روی بنر (۰ تا ۱۰۰). صفر یعنی دیده نشوند، ولی متن در HTML صفحه
        /// می‌ماند تا H1 و توضیح برای موتور جستجو و صفحه‌خوان از دست نروند.
        /// </summary>
        public int TitleOpacity { get; private set; } = MaxTitleOpacity;


        // ===== Factory =====
        public static Slide Create(
            int currentUserId,
            string firstUrl,
            string? secondUrl,
            string? bannerTitle,
            string? bannerDescription,
            string? secondUrlTextFa = null,
            string? secondUrlTextEn = null,
            int? titleOpacity = null
        )
        {
            if (string.IsNullOrWhiteSpace(firstUrl))
                throw new DomainException("آدرس صفحه مربوط به بنر دیده نشد");
            var slide = new Slide
            {
                FirstUrl = firstUrl.Trim(),
                BannerTitle = bannerTitle?.Trim() ?? string.Empty,
                BannerDescription = bannerDescription?.Trim() ?? string.Empty,
            };
            slide.SetSecondLink(secondUrl, secondUrlTextFa, secondUrlTextEn);
            slide.SetTitleOpacity(titleOpacity);
            slide.MarkCreated(currentUserId);
            return slide;
        }


        // ===== Behavior =====
        /// <summary>null یعنی «بدون تغییر»؛ رشته‌ی خالی عنوان، توضیح یا لینک دوم را پاک می‌کند.</summary>
        public void Update(
            int currentUserId,
            string? bannerUrl,
            string? firstUrl,
            string? secondUrl,
            string? bannerTitle,
            string? bannerDescription,
            string? videoUrl = null,
            string? secondUrlTextFa = null,
            string? secondUrlTextEn = null,
            int? titleOpacity = null
        )
        {
            if (!string.IsNullOrWhiteSpace(bannerUrl)) BannerUrl = bannerUrl;
            if (!string.IsNullOrWhiteSpace(firstUrl)) FirstUrl = firstUrl.Trim();
            if (bannerTitle is not null) BannerTitle = bannerTitle.Trim();
            if (bannerDescription is not null) BannerDescription = bannerDescription.Trim();
            SetSecondLink(
                secondUrl ?? SecondUrl,
                secondUrlTextFa ?? SecondUrlTextFa,
                secondUrlTextEn ?? SecondUrlTextEn);

            if (!string.IsNullOrWhiteSpace(videoUrl)) VideoUrl = videoUrl;
            SetTitleOpacity(titleOpacity);

            MarkUpdated(currentUserId);
        }

        /// <summary>
        /// آدرس دوم اختیاری است؛ اگر داده شود متن فارسی و انگلیسی آن الزامی است.
        /// بدون آدرس، متن‌ها هم پاک می‌شوند.
        /// </summary>
        /// <summary>null یعنی «بدون تغییر».</summary>
        private void SetTitleOpacity(int? opacity)
        {
            if (opacity is null) return;
            if (opacity < MinTitleOpacity || opacity > MaxTitleOpacity)
                throw new DomainException($"شفافیت عنوان باید بین {MinTitleOpacity} تا {MaxTitleOpacity} باشد");
            TitleOpacity = opacity.Value;
        }

        private void SetSecondLink(string? url, string? textFa, string? textEn)
        {
            var cleanUrl = url?.Trim() ?? string.Empty;
            if (cleanUrl.Length == 0)
            {
                SecondUrl = string.Empty;
                SecondUrlTextFa = string.Empty;
                SecondUrlTextEn = string.Empty;
                return;
            }

            var fa = textFa?.Trim() ?? string.Empty;
            var en = textEn?.Trim() ?? string.Empty;
            if (fa.Length == 0 || en.Length == 0)
                throw new DomainException("برای آدرس دوم، متن فارسی و انگلیسی لینک الزامی است");
            if (fa.Length > MaxLinkTextLength || en.Length > MaxLinkTextLength)
                throw new DomainException($"متن لینک دوم حداکثر {MaxLinkTextLength} کاراکتر است");

            SecondUrl = cleanUrl;
            SecondUrlTextFa = fa;
            SecondUrlTextEn = en;
        }

        public void SetMobileMedia(int currentUserId, string? mobileBannerUrl, string? mobileVideoUrl)
        {
            if (!string.IsNullOrWhiteSpace(mobileBannerUrl)) MobileBannerUrl = mobileBannerUrl;
            if (!string.IsNullOrWhiteSpace(mobileVideoUrl)) MobileVideoUrl = mobileVideoUrl;
            MarkUpdated(currentUserId);
        }

        public void ClearMobileBanner(int currentUserId)
        {
            MobileBannerUrl = string.Empty;
            MarkUpdated(currentUserId);
        }

        public void ClearMobileVideo(int currentUserId)
        {
            MobileVideoUrl = string.Empty;
            MarkUpdated(currentUserId);
        }

        public void ClearVideo(int currentUserId)
        {
            VideoUrl = string.Empty;
            MarkUpdated(currentUserId);
        }

        public void SetHero(bool isHero, int currentUserId)
        {
            IsHero = isHero;
            MarkUpdated(currentUserId);
        }

    }
}
