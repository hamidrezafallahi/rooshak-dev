using OnlineShop.Domain.Common;
namespace OnlineShop.Domain.Entities
{
    public class Category : BaseEntity
    {
        private Category() { } 

        public string PersianName { get; private set; } = string.Empty;
        public string EnglishName { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;

        public int? ParentCategoryId { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string CategoryPersianDesc { get; private set; }
        public string CategoryEnglishDesc { get; private set; }
        public bool IsShowInLanding { get; private set; }=false;
        public string? SeoTitleFa { get; private set; }
        public string? SeoTitleEn { get; private set; }
        public string? MetaDescriptionFa { get; private set; }
        public string? MetaDescriptionEn { get; private set; }
        public string? FaqJson { get; private set; }


        public Category? ParentCategory { get; private set; }
        private readonly List<Category> _subCategories = new();
        public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        public static Category Create(
            string persianName,
            string persianDesc,
            string englishName,
            string englishDesc,
            bool isShowInLanding,
            bool? isActive,
            int? parentCategoryId,
            int currentUserId,
            string? slug = null,
            string? seoTitleFa = null,
            string? seoTitleEn = null,
            string? metaDescriptionFa = null,
            string? metaDescriptionEn = null,
            string? faqJson = null)
        {
            if (string.IsNullOrWhiteSpace(persianName))
                throw new DomainException("نام فارسی دسته‌بندی نمی‌تواند خالی باشد.");

            var category = new Category
            {
                PersianName = persianName,
                EnglishName = englishName,
                CategoryPersianDesc = persianDesc,
                CategoryEnglishDesc = englishDesc,
                IsShowInLanding= isShowInLanding,
                ParentCategoryId = parentCategoryId,
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(
                    string.IsNullOrWhiteSpace(slug)
                        ? (string.IsNullOrWhiteSpace(englishName) ? persianName : englishName)
                        : slug),
                SeoTitleFa = NullIfWhiteSpace(seoTitleFa),
                SeoTitleEn = NullIfWhiteSpace(seoTitleEn),
                MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa),
                MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn),
                FaqJson = NullIfWhiteSpace(faqJson),
            };
            if (isActive.HasValue)
                category.SetActive(isActive.Value, currentUserId);


            category.MarkCreated(currentUserId);
            return category;
        }

        public void Update(
            string? persianName,
            string? persianDesc,
            string? englishName,
            string? englishDesc,
            string? imageUrl,
            bool? showInLanding,
            int? parentCategoryId,
            int currentUserId,
            string? slug = null,
            string? seoTitleFa = null,
            string? seoTitleEn = null,
            string? metaDescriptionFa = null,
            string? metaDescriptionEn = null,
            string? faqJson = null)
        {
            if (!string.IsNullOrWhiteSpace(persianName))
                PersianName = persianName;
            if (!string.IsNullOrWhiteSpace(persianDesc))
                CategoryPersianDesc = persianDesc;
            if (!string.IsNullOrWhiteSpace(englishName))
                EnglishName = englishName;
            if (!string.IsNullOrWhiteSpace(englishDesc))
                CategoryEnglishDesc = englishDesc;
            if (!string.IsNullOrWhiteSpace(imageUrl))
                ImageUrl = imageUrl;
            if (showInLanding.HasValue)
                IsShowInLanding = showInLanding.Value;
            if (parentCategoryId.HasValue)
                ParentCategoryId = parentCategoryId.Value;

            if (!string.IsNullOrWhiteSpace(slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(slug);
            else if (string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(
                    string.IsNullOrWhiteSpace(EnglishName) ? PersianName : EnglishName);

            if (seoTitleFa != null) SeoTitleFa = NullIfWhiteSpace(seoTitleFa);
            if (seoTitleEn != null) SeoTitleEn = NullIfWhiteSpace(seoTitleEn);
            if (metaDescriptionFa != null) MetaDescriptionFa = NullIfWhiteSpace(metaDescriptionFa);
            if (metaDescriptionEn != null) MetaDescriptionEn = NullIfWhiteSpace(metaDescriptionEn);
            if (faqJson != null) FaqJson = NullIfWhiteSpace(faqJson);

            MarkUpdated(currentUserId);
        }

        public void EnsureSlug(int? uniqueSuffix = null)
        {
            var source = string.IsNullOrWhiteSpace(EnglishName) ? PersianName : EnglishName;
            if (string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(source, uniqueSuffix);
            else if (uniqueSuffix.HasValue && !Slug.EndsWith($"-{uniqueSuffix.Value}", StringComparison.Ordinal))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(Slug, uniqueSuffix);
        }

        public void AddSubCategory(Category subCategory, int currentUserId)
        {
            if (subCategory is null)
                throw new ArgumentNullException(nameof(subCategory));
            if (!_subCategories.Contains(subCategory))
                _subCategories.Add(subCategory);
            MarkUpdated(currentUserId);
        }

        public void RemoveSubCategory(Category subCategory, int currentUserId)
        {
            if (subCategory is null)
                throw new ArgumentNullException(nameof(subCategory));

            _subCategories.Remove(subCategory);
            MarkUpdated(currentUserId);
        }

        public void AddProduct(Product product, int currentUserId)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            _products.Add(product);
            MarkUpdated(currentUserId);
        }

        public void RemoveProduct(Product product, int currentUserId)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            _products.Remove(product);
            MarkUpdated(currentUserId);
        }

        public void Delete(int currentUserId)
        {
            foreach (var sub in _subCategories)
            {
                sub.Delete(currentUserId);
            }

            MarkDeleted(currentUserId);
        }

        private static string? NullIfWhiteSpace(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
