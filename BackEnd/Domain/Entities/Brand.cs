using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Brand : BaseEntity
    {
        private Brand() { }

        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string LogoUrl { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string? SeoTitleFa { get; private set; }
        public string? SeoTitleEn { get; private set; }
        public string? MetaDescriptionFa { get; private set; }
        public string? MetaDescriptionEn { get; private set; }

        public ICollection<Product> Products { get; private set; } = new List<Product>();

        public static Brand Create(string name, string description, int currentUserId, string? slug = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("نام برند نمی‌تواند خالی باشد.");

            var brand = new Brand
            {
                Name = name,
                Description = description,
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(
                    string.IsNullOrWhiteSpace(slug) ? name : slug),
            };
            brand.SetActive(true, currentUserId);
            brand.MarkCreated(currentUserId);
            return brand;
        }

        public void Update(
            int currentUserId,
            string? name,
            string? logoUrl,
            string? description,
            bool? isActive,
            string? slug = null,
            string? seoTitleFa = null,
            string? seoTitleEn = null,
            string? metaDescriptionFa = null,
            string? metaDescriptionEn = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (!string.IsNullOrWhiteSpace(logoUrl))
                LogoUrl = logoUrl;

            if (!string.IsNullOrWhiteSpace(description))
                Description = description;

            if (!string.IsNullOrWhiteSpace(slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(slug);
            else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(name);

            if (seoTitleFa != null) SeoTitleFa = NullIfWhiteSpace(seoTitleFa);
            if (seoTitleEn != null) SeoTitleEn = NullIfWhiteSpace(seoTitleEn);
            if (metaDescriptionFa != null) MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa);
            if (metaDescriptionEn != null) MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn);

            if (isActive.HasValue) SetActive(isActive.Value, currentUserId);
            MarkUpdated(currentUserId);
        }

        public void EnsureSlug(int? uniqueSuffix = null)
        {
            if (string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(Name, uniqueSuffix);
            else if (uniqueSuffix.HasValue && !Slug.EndsWith($"-{uniqueSuffix.Value}", StringComparison.Ordinal))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(Slug, uniqueSuffix);
        }

        public void SetSeo(
            string? seoTitleFa,
            string? seoTitleEn,
            string? metaDescriptionFa,
            string? metaDescriptionEn,
            int currentUserId)
        {
            if (seoTitleFa != null) SeoTitleFa = NullIfWhiteSpace(seoTitleFa);
            if (seoTitleEn != null) SeoTitleEn = NullIfWhiteSpace(seoTitleEn);
            if (metaDescriptionFa != null) MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa);
            if (metaDescriptionEn != null) MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn);
            MarkUpdated(currentUserId);
        }

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
