using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    /// <summary>
    /// فونتی که ادمین خودش آپلود می‌کند (جدول CustomFonts). فایل در uploads/fonts/{id} و مشخصاتش
    /// در دیتابیس است، پس با بک‌آپ و انتقال سرور همراه بقیه‌ی محتوا جابه‌جا می‌شود.
    /// کلید آن در تم «custom-{Id}» است (<see cref="KeyFor"/>).
    /// </summary>
    public class CustomFont : BaseEntity
    {
        public const string KeyPrefix = "custom-";
        public const int MaxNameLength = 60;

        public static readonly IReadOnlyCollection<string> Categories = new[] { "general", "formal", "fancy" };

        private CustomFont() { }

        public string NameFa { get; private set; } = string.Empty;
        public string NameEn { get; private set; } = string.Empty;
        /// <summary>general / formal / fancy (همان سه دسته‌ی فونت‌های آماده).</summary>
        public string Category { get; private set; } = "general";
        public string FileUrl { get; private set; } = string.Empty;

        public string Key => KeyFor(Id);

        public static string KeyFor(int id) => $"{KeyPrefix}{id}";

        /// <summary>شناسه‌ی فونت از روی کلید «custom-{Id}»؛ null اگر کلید از این نوع نباشد.</summary>
        public static int? IdFromKey(string? key)
        {
            if (key is null || !key.StartsWith(KeyPrefix, StringComparison.Ordinal))
                return null;
            return int.TryParse(key.AsSpan(KeyPrefix.Length), out var id) && id > 0 ? id : null;
        }

        public static CustomFont Create(string? nameFa, string? nameEn, string? category, int currentUserId)
        {
            var font = new CustomFont();
            font.Apply(nameFa, nameEn, category);
            font.MarkCreated(currentUserId);
            return font;
        }

        public void Update(string? nameFa, string? nameEn, string? category, int currentUserId)
        {
            Apply(
                string.IsNullOrWhiteSpace(nameFa) ? NameFa : nameFa,
                string.IsNullOrWhiteSpace(nameEn) ? NameEn : nameEn,
                string.IsNullOrWhiteSpace(category) ? Category : category);
            MarkUpdated(currentUserId);
        }

        public void SetFile(string fileUrl, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new DomainException("فایل فونت الزامی است");
            FileUrl = fileUrl;
            MarkUpdated(currentUserId);
        }

        private void Apply(string? nameFa, string? nameEn, string? category)
        {
            var fa = nameFa?.Trim() ?? string.Empty;
            var en = nameEn?.Trim() ?? string.Empty;
            if (fa.Length == 0 && en.Length == 0)
                throw new DomainException("نام فونت الزامی است");
            if (fa.Length > MaxNameLength || en.Length > MaxNameLength)
                throw new DomainException($"نام فونت حداکثر {MaxNameLength} کاراکتر است");

            var cat = category?.Trim().ToLowerInvariant() ?? "general";
            if (!Categories.Contains(cat))
                throw new DomainException("دسته‌ی فونت باید general، formal یا fancy باشد");

            NameFa = fa.Length > 0 ? fa : en;
            NameEn = en.Length > 0 ? en : fa;
            Category = cat;
        }
    }
}
