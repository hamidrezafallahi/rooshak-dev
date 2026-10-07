using System.Text.RegularExpressions;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// پالت رنگی سایت. فرانت‌اند رنگ‌ها را از رکورد فعال همین جدول می‌خواند و به صورت
    /// CSS variable روی صفحه می‌نشاند؛ هیچ رنگی در فایل CSS قفل نشده است.
    /// همیشه حداکثر یک تم فعال است (قاعده در ThemeSettingCommandHandler اعمال می‌شود).
    /// </summary>
    public class ThemeSetting : BaseEntity
    {
        private static readonly Regex HexColor =
            new("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private ThemeSetting() { }

        public string Name { get; private set; } = string.Empty;

        // ===== رنگ‌های برند و وضعیت =====
        public string PrimaryColor { get; private set; } = string.Empty;
        public string SecondaryColor { get; private set; } = string.Empty;
        public string HighlightColor { get; private set; } = string.Empty;
        public string NeutralColor { get; private set; } = string.Empty;
        public string SuccessColor { get; private set; } = string.Empty;
        public string ErrorColor { get; private set; } = string.Empty;
        public string WarningColor { get; private set; } = string.Empty;
        public string InfoColor { get; private set; } = string.Empty;

        // ===== رنگ‌های سطوح و متن فروشگاه =====
        public string SurfaceColor { get; private set; } = string.Empty;
        public string SurfaceMutedColor { get; private set; } = string.Empty;
        public string BorderColor { get; private set; } = string.Empty;
        public string TextColor { get; private set; } = string.Empty;
        public string TextMutedColor { get; private set; } = string.Empty;

        public static bool IsValidHex(string? value) =>
            !string.IsNullOrWhiteSpace(value) && HexColor.IsMatch(value.Trim());

        // ===== Factory =====
        public static ThemeSetting Create(
            string name,
            ThemePalette palette,
            int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام تم الزامی است", nameof(name));

            var theme = new ThemeSetting { Name = name.Trim() };
            theme.ApplyPalette(palette);
            theme.MarkCreated(currentUserId);
            return theme;
        }

        // ===== Behavior =====
        public void Update(string? name, ThemePalette palette, int currentUserId)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            ApplyPalette(palette);
            MarkUpdated(currentUserId);
        }

        /// <summary>فقط مقادیر ارسال‌شده (غیرخالی) عوض می‌شوند؛ همه باید hex شش‌رقمی باشند.</summary>
        private void ApplyPalette(ThemePalette p)
        {
            PrimaryColor = Pick(PrimaryColor, p.Primary, nameof(PrimaryColor));
            SecondaryColor = Pick(SecondaryColor, p.Secondary, nameof(SecondaryColor));
            HighlightColor = Pick(HighlightColor, p.Highlight, nameof(HighlightColor));
            NeutralColor = Pick(NeutralColor, p.Neutral, nameof(NeutralColor));
            SuccessColor = Pick(SuccessColor, p.Success, nameof(SuccessColor));
            ErrorColor = Pick(ErrorColor, p.Error, nameof(ErrorColor));
            WarningColor = Pick(WarningColor, p.Warning, nameof(WarningColor));
            InfoColor = Pick(InfoColor, p.Info, nameof(InfoColor));
            SurfaceColor = Pick(SurfaceColor, p.Surface, nameof(SurfaceColor));
            SurfaceMutedColor = Pick(SurfaceMutedColor, p.SurfaceMuted, nameof(SurfaceMutedColor));
            BorderColor = Pick(BorderColor, p.Border, nameof(BorderColor));
            TextColor = Pick(TextColor, p.Text, nameof(TextColor));
            TextMutedColor = Pick(TextMutedColor, p.TextMuted, nameof(TextMutedColor));
        }

        private static string Pick(string current, string? incoming, string field)
        {
            if (string.IsNullOrWhiteSpace(incoming))
            {
                if (string.IsNullOrEmpty(current))
                    throw new ArgumentException($"رنگ {field} الزامی است", field);
                return current;
            }

            if (!IsValidHex(incoming))
                throw new ArgumentException($"رنگ {field} باید به شکل #RRGGBB باشد", field);

            return incoming.Trim().ToLowerInvariant();
        }
    }

    /// <summary>مجموعه‌ی رنگ‌های یک تم (value object برای جلوگیری از لیست طولانی پارامتر).</summary>
    public sealed record ThemePalette(
        string? Primary,
        string? Secondary,
        string? Highlight,
        string? Neutral,
        string? Success,
        string? Error,
        string? Warning,
        string? Info,
        string? Surface,
        string? SurfaceMuted,
        string? Border,
        string? Text,
        string? TextMuted);
}
