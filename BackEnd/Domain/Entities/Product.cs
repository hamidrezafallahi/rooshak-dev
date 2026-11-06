using Domain.Entities;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.ValueObjects;


namespace OnlineShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        private Product() { }

        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string? SeoTitleFa { get; private set; }
        public string? SeoTitleEn { get; private set; }
        public string? MetaDescriptionFa { get; private set; }
        public string? MetaDescriptionEn { get; private set; }

        // ==== Category ====
        public int CategoryId { get; private set; }
        public Category Category { get; private set; }

        // ==== Brand ====
        public int? BrandId { get; private set; }
        public Brand? Brand { get; private set; }

        // ==== Images ====

        public ICollection<ProductImage> Images { get; private set; } = new List<ProductImage>();



        // ==== Specifications ====

        public ICollection<ProductSpecification> Specifications { get; private set; } = new List<ProductSpecification>();
        public ICollection<ProductOffers> ProductOffers { get; private set; } = new List<ProductOffers>();

        // ===== Dimensions =====
        public ProductDimensions? Dimensions { get; private set; }
 

        // ===== Factory Method =====
        public static Product Create(
            string name,
            string description,
            int categoryId,
            int brandId,
            ProductDimensions dimensions,
            int currentUserId,
            string? slug = null,
            string? seoTitleFa = null,
            string? seoTitleEn = null,
            string? metaDescriptionFa = null,
            string? metaDescriptionEn = null)

        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("نام محصول نمی‌تواند خالی باشد.");

            var product = new Product
            {
                Name = name,
                Slug = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Generate(name) : SlugHelper.Generate(slug),
                CategoryId = categoryId,
                BrandId = brandId,
                Description = description,
                Dimensions = dimensions,
                SeoTitleFa = NullIfWhiteSpace(seoTitleFa),
                SeoTitleEn = NullIfWhiteSpace(seoTitleEn),
                MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa),
                MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn),
            };

            product.MarkCreated(currentUserId);

            return product;
        }

        // ===== Behavior Methods =====
        public void Update(
            int currentUserId,
            string? name,
            string? description,
            int? categoryId,
            int? brandId,
            ProductDimensions? dimensions,
            string? slug = null,
            string? seoTitleFa = null,
            string? seoTitleEn = null,
            string? metaDescriptionFa = null,
            string? metaDescriptionEn = null
            )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (categoryId.HasValue)
                CategoryId = categoryId.Value;

            if (brandId.HasValue)
                BrandId = brandId.Value;

            if (description != null)
                Description = description;

            if (dimensions != null)
                Dimensions = dimensions;

            if (!string.IsNullOrWhiteSpace(slug))
                Slug = SlugHelper.Generate(slug);
            else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(Slug))
                Slug = SlugHelper.Generate(name);

            if (seoTitleFa != null) SeoTitleFa = NullIfWhiteSpace(seoTitleFa);
            if (seoTitleEn != null) SeoTitleEn = NullIfWhiteSpace(seoTitleEn);
            if (metaDescriptionFa != null) MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa);
            if (metaDescriptionEn != null) MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn);

            MarkUpdated(currentUserId);
        }

        public void EnsureSlug(int? uniqueSuffix = null)
        {
            if (string.IsNullOrWhiteSpace(Slug))
                Slug = SlugHelper.Generate(Name, uniqueSuffix);
            else if (uniqueSuffix.HasValue && !Slug.EndsWith($"-{uniqueSuffix.Value}", StringComparison.Ordinal))
                Slug = SlugHelper.Generate(Slug, uniqueSuffix);
        }


        public void AddImage(ProductImage image, int currentUserId)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Images.Add(image);
            MarkUpdated(currentUserId);
        }

        public void RemoveImage(ProductImage image, int currentUserId)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            Images.Remove(image);
            MarkUpdated(currentUserId);
        }
 

        public void SetDimensions(ProductDimensions dimensions)
        {
            Dimensions = dimensions;

        }

        public void AddSpecification(string key, string value)
        {
            if (Specifications.Any(s => s.Key == key))
                throw new DomainException("این مشخصه قبلاً برای محصول ثبت شده است.");

            Specifications.Add(
                ProductSpecification.Create(this.Id, key, value)
            );

        }

        public void RemoveSpecification(string key, int userId)
        {
            var spec = Specifications.FirstOrDefault(s => s.Key == key);
            if (spec != null)
            {
                Specifications.Remove(spec);
                MarkUpdated(userId);
            }
        }

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    }
}
