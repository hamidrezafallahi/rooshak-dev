
namespace OnlineShop.Domain.Entities
{
    public class Slide : BaseEntity
    {
        private Slide() { }

        // ===== محتوای فارسی =====
        public string BannerUrl { get; private set; } = string.Empty;
        public string BannerTitle { get; private set; } = string.Empty;
        public string BannerDescription { get; private set; } = string.Empty;
        public string FirstUrl { get; private set; } = string.Empty;
        public string SecondUrl { get; private set; } = string.Empty;
        /// <summary>Optional hero video (mp4/webm). BannerUrl doubles as its poster / fallback image.</summary>
        public string VideoUrl { get; private set; } = string.Empty;
        /// <summary>نسخه‌ی موبایل: پوستر (عکس) و ویدیوی جدا برای صفحه‌های کوچک. خالی = از نسخه‌ی دسکتاپ استفاده می‌شود.</summary>
        public string MobileBannerUrl { get; private set; } = string.Empty;
        public string MobileVideoUrl { get; private set; } = string.Empty;
        public bool IsHero { get; private set; } 


        // ===== Factory =====
        public static Slide Create(
            int currentUserId,
            string firstUrl,
            string? secondUrl,
            string bannerTitle,
            string bannerDescription
        )
        {
            if (string.IsNullOrWhiteSpace(firstUrl))
                throw new ArgumentException("آدرس صفحه مربوط به بنر دیده نشد");
            var slide = new Slide
            {
                FirstUrl = firstUrl ?? string.Empty,
                SecondUrl = secondUrl ?? string.Empty,
                BannerTitle = bannerTitle ?? string.Empty,
                BannerDescription = bannerDescription ?? string.Empty,
            };
            slide.MarkCreated(currentUserId);
            return slide;
        }


        // ===== Behavior =====
        public void Update(
            int currentUserId,
            string? bannerUrl,
            string? firstUrl,
            string? secondUrl,
            string? bannerTitle,
            string? bannerDescription,
            string? videoUrl = null
        )
        {
            if (!string.IsNullOrWhiteSpace(bannerUrl)) BannerUrl = bannerUrl;
            if (!string.IsNullOrWhiteSpace(firstUrl)) FirstUrl = firstUrl;
            // Empty string is allowed to clear an optional second URL
            if (secondUrl is not null) SecondUrl = secondUrl;
            if (!string.IsNullOrWhiteSpace(bannerTitle)) BannerTitle = bannerTitle;
            if (!string.IsNullOrWhiteSpace(bannerDescription)) BannerDescription = bannerDescription;

            if (!string.IsNullOrWhiteSpace(videoUrl)) VideoUrl = videoUrl;


            MarkUpdated(currentUserId);
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
