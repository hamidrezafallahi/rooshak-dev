using OnlineShop.Domain.Common;
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

        // ===== تایپوگرافی =====
        /// <summary>کلید فونت اصلی سایت از فهرست ثابت <see cref="ThemeFonts"/>؛ فرانت‌اند آن را به فونت واقعی نگاشت می‌کند.</summary>
        public string FontFamily { get; private set; } = ThemeFonts.Default;
        /// <summary>کلید فونت تیترها از همان فهرست ثابت؛ پیش‌فرض همان فونت فعلی سایت (ایران‌سنس).</summary>
        public string HeadingFontFamily { get; private set; } = ThemeFonts.DefaultHeading;

        public static bool IsValidHex(string? value) =>
            !string.IsNullOrWhiteSpace(value) && HexColor.IsMatch(value.Trim());

        // ===== Factory =====
        public static ThemeSetting Create(
            string name,
            ThemePalette palette,
            int currentUserId,
            string? fontFamily = null,
            string? headingFontFamily = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("نام تم الزامی است");

            var theme = new ThemeSetting { Name = name.Trim() };
            theme.ApplyPalette(palette);
            theme.ApplyFonts(fontFamily, headingFontFamily);
            theme.MarkCreated(currentUserId);
            return theme;
        }

        // ===== Behavior =====
        public void Update(string? name, ThemePalette palette, int currentUserId, string? fontFamily = null, string? headingFontFamily = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            ApplyPalette(palette);
            ApplyFonts(fontFamily, headingFontFamily);
            MarkUpdated(currentUserId);
        }

        private void ApplyFonts(string? fontFamily, string? headingFontFamily)
        {
            FontFamily = PickFont(FontFamily, fontFamily, ThemeFonts.Default);
            HeadingFontFamily = PickFont(HeadingFontFamily, headingFontFamily, ThemeFonts.DefaultHeading);
        }

        /// <summary>خالی یعنی «تغییر نده»؛ فقط کلیدهای فهرست ثابت پذیرفته می‌شوند.</summary>
        private static string PickFont(string current, string? incoming, string fallback)
        {
            if (string.IsNullOrWhiteSpace(incoming))
                return string.IsNullOrWhiteSpace(current) ? fallback : current;

            var key = incoming.Trim().ToLowerInvariant();
            if (!ThemeFonts.IsValid(key))
                throw new DomainException("فونت انتخاب‌شده در فهرست فونت‌های مجاز نیست");

            return key;
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
                    throw new DomainException($"رنگ {field} الزامی است");
                return current;
            }

            if (!IsValidHex(incoming))
                throw new DomainException($"رنگ {field} باید به شکل #RRGGBB باشد");

            return incoming.Trim().ToLowerInvariant();
        }
    }

    /// <summary>
    /// فهرست ثابت فونت‌های قابل انتخاب در پنل ادمین. باید با FrontEnd/lib/fontCatalog.ts یکی بماند؛
    /// کلید ناشناخته ذخیره نمی‌شود تا هیچ مقدار دلخواهی به CSS سایت نرسد.
    /// </summary>
    public static class ThemeFonts
    {
        // روشاک امروز هم متن و هم تیترها را با ایران‌سنس نشان می‌دهد.
        public const string Default = "iransans";
        public const string DefaultHeading = "iransans";
        public const int MaxLength = 40;

        public static readonly IReadOnlyCollection<string> Keys = new HashSet<string>(StringComparer.Ordinal)
        {
            // عمومی
            "vazirmatn", "iransans", "noto-sans-arabic", "harmattan", "mada", "tajawal", "cairo", "readex-pro",
            // رسمی
            "noto-naskh-arabic", "amiri", "scheherazade-new", "lateef", "markazi-text",
            // فانتزی و تزئینی
            "lalezar", "reem-kufi", "el-messiri", "changa", "aref-ruqaa", "rakkas", "lemonada", "noto-nastaliq-urdu",
        };

        public static bool IsValid(string? key) => key != null && Keys.Contains(key);
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
