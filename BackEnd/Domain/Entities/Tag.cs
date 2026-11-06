using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class Tag : BaseEntity
    {
        private Tag() { }

        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public ICollection<ProductOfferTag> ProductOfferTags { get; set; } = new List<ProductOfferTag>();
        public ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
        public ICollection<UserTag> UserTags { get; set; } = new List<UserTag>();

        public static Tag Create(string name, int currentUserId, string? slug = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("نام برچسب نمی‌تواند خالی باشد.");

            var tag = new Tag
            {
                Name = name,
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(
                    string.IsNullOrWhiteSpace(slug) ? name : slug),
            };

            tag.MarkCreated(currentUserId);
            return tag;
        }

        public void Update(string name, int currentUserId, string? slug = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (!string.IsNullOrWhiteSpace(slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(slug);
            else if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(name);

            MarkUpdated(currentUserId);
        }

        public void EnsureSlug(int? uniqueSuffix = null)
        {
            if (string.IsNullOrWhiteSpace(Slug))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(Name, uniqueSuffix);
            else if (uniqueSuffix.HasValue && !Slug.EndsWith($"-{uniqueSuffix.Value}", StringComparison.Ordinal))
                Slug = OnlineShop.Domain.Common.SlugHelper.Generate(Slug, uniqueSuffix);
        }
    }
}
