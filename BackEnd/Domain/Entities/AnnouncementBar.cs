using OnlineShop.Domain.Common;
namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// نوار اعلان بالای هدر سایت: متن (فارسی/انگلیسی)، تصویر پس‌زمینه، رنگ‌ها، ارتفاع،
    /// لینک و بازه‌ی زمانی نمایش. IsActive همان «نمایش بده / نده» است.
    /// </summary>
    public class AnnouncementBar : BaseEntity
    {
        public const int MinHeightPx = 24;
        public const int MaxHeightPx = 160;

        private AnnouncementBar() { }

        public string MessageFa { get; private set; } = string.Empty;
        public string MessageEn { get; private set; } = string.Empty;

        /// <summary>آدرس مقصد کلیک (داخلی مثل products یا کامل مثل https://...). خالی = بدون لینک.</summary>
        public string LinkUrl { get; private set; } = string.Empty;

        public string BackgroundImageUrl { get; private set; } = string.Empty;
        public string BackgroundColor { get; private set; } = "#000000";
        public string TextColor { get; private set; } = "#ffffff";
        public int HeightPx { get; private set; } = 36;

        /// <summary>شروع/پایان نمایش (UTC). null یعنی بدون محدودیت.</summary>
        public DateTime? StartsAt { get; private set; }
        public DateTime? EndsAt { get; private set; }

        /// <summary>اگر چند نوار هم‌زمان معتبر باشند، عدد کوچک‌تر نمایش داده می‌شود.</summary>
        public int DisplayOrder { get; private set; }

        public static AnnouncementBar Create(
            string messageFa,
            string? messageEn,
            string? linkUrl,
            string? backgroundColor,
            string? textColor,
            int? heightPx,
            DateTime? startsAt,
            DateTime? endsAt,
            int? displayOrder,
            int currentUserId)
        {
            var bar = new AnnouncementBar();
            bar.Apply(messageFa, messageEn, linkUrl, backgroundColor, textColor, heightPx,
                startsAt, endsAt, displayOrder, replaceSchedule: true);
            bar.MarkCreated(currentUserId);
            return bar;
        }

        public void Update(
            string? messageFa,
            string? messageEn,
            string? linkUrl,
            string? backgroundColor,
            string? textColor,
            int? heightPx,
            DateTime? startsAt,
            DateTime? endsAt,
            int? displayOrder,
            int currentUserId)
        {
            Apply(
                string.IsNullOrWhiteSpace(messageFa) ? MessageFa : messageFa,
                messageEn ?? MessageEn,
                linkUrl ?? LinkUrl,
                backgroundColor, textColor, heightPx,
                startsAt, endsAt, displayOrder, replaceSchedule: true);
            MarkUpdated(currentUserId);
        }

        public void SetBackgroundImage(string? url, int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(url)) BackgroundImageUrl = url;
            MarkUpdated(currentUserId);
        }

        public void ClearBackgroundImage(int currentUserId)
        {
            BackgroundImageUrl = string.Empty;
            MarkUpdated(currentUserId);
        }

        /// <summary>آیا این نوار در لحظه‌ی داده‌شده باید نمایش داده شود؟</summary>
        public bool IsVisibleAt(DateTime utcNow) =>
            IsActive
            && !IsDeleted
            && (StartsAt is null || StartsAt <= utcNow)
            && (EndsAt is null || EndsAt >= utcNow);

        private void Apply(
            string messageFa,
            string? messageEn,
            string? linkUrl,
            string? backgroundColor,
            string? textColor,
            int? heightPx,
            DateTime? startsAt,
            DateTime? endsAt,
            int? displayOrder,
            bool replaceSchedule)
        {
            if (string.IsNullOrWhiteSpace(messageFa))
                throw new DomainException("متن فارسی نوار اعلان الزامی است");

            if (heightPx is { } h && (h < MinHeightPx || h > MaxHeightPx))
                throw new DomainException($"ارتفاع نوار باید بین {MinHeightPx} تا {MaxHeightPx} پیکسل باشد");

            if (!string.IsNullOrWhiteSpace(backgroundColor) && !ThemeSetting.IsValidHex(backgroundColor))
                throw new DomainException("رنگ پس‌زمینه باید به شکل #RRGGBB باشد");

            if (!string.IsNullOrWhiteSpace(textColor) && !ThemeSetting.IsValidHex(textColor))
                throw new DomainException("رنگ متن باید به شکل #RRGGBB باشد");

            var start = replaceSchedule ? ToUtc(startsAt) : StartsAt;
            var end = replaceSchedule ? ToUtc(endsAt) : EndsAt;
            if (start is not null && end is not null && end < start)
                throw new DomainException("پایان نمایش نمی‌تواند قبل از شروع باشد");

            MessageFa = messageFa.Trim();
            MessageEn = (messageEn ?? string.Empty).Trim();
            LinkUrl = (linkUrl ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(backgroundColor)) BackgroundColor = backgroundColor.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(textColor)) TextColor = textColor.Trim().ToLowerInvariant();
            if (heightPx.HasValue) HeightPx = heightPx.Value;
            StartsAt = start;
            EndsAt = end;
            if (displayOrder.HasValue) DisplayOrder = displayOrder.Value;
        }

        private static DateTime? ToUtc(DateTime? value)
        {
            if (value is null) return null;
            return value.Value.Kind == DateTimeKind.Utc ? value : value.Value.ToUniversalTime();
        }
    }
}
