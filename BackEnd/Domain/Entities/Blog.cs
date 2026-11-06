using OnlineShop.Domain.Common;
namespace OnlineShop.Domain.Entities
{
    public class Blog : BaseEntity
    {
        private Blog() { }

        // ===== محتوای فارسی =====
        public string TitleFa { get; private set; } = null!;
        public string IntroFa { get; private set; } = string.Empty;
        public string ContentFa { get; private set; } = string.Empty;
        public string ConclusionFa { get; private set; } = string.Empty;
        public string? ExcerptFa { get; private set; }

        // ===== محتوای انگلیسی =====
        public string TitleEn { get; private set; } = null!;
        public string IntroEn { get; private set; } = string.Empty;
        public string ContentEn { get; private set; } = string.Empty;
        public string ConclusionEn { get; private set; } = string.Empty;
        public string? ExcerptEn { get; private set; }

        // ===== SEO Meta =====
        public string? MetaDescriptionFa { get; private set; }
        public string? MetaDescriptionEn { get; private set; }
        public string? MetaKeywordsFa { get; private set; }
        public string? MetaKeywordsEn { get; private set; }

        // ===== سایر =====
        public string Slug { get; private set; } = string.Empty;
        public string? ThumbnailFile { get; private set; }

        public int AuthorId { get; private set; }
        public User Author { get; private set; } = default!;
        public ICollection<BlogTag> BlogTags { get; private set; } = new List<BlogTag>();

        // ===== Factory =====

 
        public static Blog Create(
            string titleFa,
            string introFa,
            string contentFa,
            string conclusionFa,
            string excerptFa,
            string metaDescriptionFa,
            string metaKeywordsFa,
            string titleEn,
            string introEn,
            string contentEn,
            string conclusionEn,
            string excerptEn,
            string metaDescriptionEn,
            string metaKeywordsEn,       
            string slug,
            string thumbnailFile,
            int authorId,
            int currentUserId
            )
        {
            if (string.IsNullOrWhiteSpace(titleFa) && string.IsNullOrWhiteSpace(titleEn))
                throw new DomainException("حداقل یکی از عنوان‌های فارسی یا انگلیسی مقاله باید وارد شود.");

            var blog = new Blog
            {
                TitleFa = titleFa,
                IntroFa = introFa,
                ContentFa = contentFa ?? string.Empty,
                ConclusionFa = conclusionFa ?? string.Empty,
                ExcerptFa = excerptFa,
                MetaDescriptionFa = metaDescriptionFa,
                MetaKeywordsFa = metaKeywordsFa,

                TitleEn = titleEn ?? string.Empty,
                IntroEn = introEn ?? string.Empty,
                ContentEn = contentEn ?? string.Empty,
                ConclusionEn = conclusionEn ?? string.Empty,
                ExcerptEn = excerptEn,
                MetaDescriptionEn = metaDescriptionEn,
                MetaKeywordsEn = metaKeywordsEn,

                Slug = slug ?? GenerateSlug(titleFa ?? titleEn ?? ""),
                ThumbnailFile = "",
                AuthorId = authorId,
 
            };

            blog.MarkCreated(currentUserId);
            return blog;
        }

        // ===== Behavior =====
        public void Update(
            int currentUserId,
            string? titleFa = null,
            string? introFa = null,
            string? contentFa = null,
            string? conclusionFa = null,
            string? excerptFa = null,
            string? metaDescriptionFa = null,
            string? metaKeywordsFa = null,
            string? titleEn = null,
            string? introEn = null,
            string? contentEn = null,
            string? conclusionEn = null,
            string? excerptEn = null,
            string? metaDescriptionEn = null,
            string? metaKeywordsEn = null,
            string? slug = null,
            string? thumbnailFile = null,
            int? authorId = null
  

        )
        {
            // فارسی
            if (!string.IsNullOrWhiteSpace(titleFa)) TitleFa = titleFa;
            if (!string.IsNullOrWhiteSpace(introFa)) IntroFa = introFa;
            if (!string.IsNullOrWhiteSpace(contentFa)) ContentFa = contentFa;
            if (!string.IsNullOrWhiteSpace(conclusionFa)) ConclusionFa = conclusionFa;
            if (excerptFa != null) ExcerptFa = excerptFa;
            if (metaDescriptionFa != null) MetaDescriptionFa = metaDescriptionFa;
            if (metaKeywordsFa != null) MetaKeywordsFa = metaKeywordsFa;

            // انگلیسی
            if (!string.IsNullOrWhiteSpace(titleEn)) TitleEn = titleEn;
            if (!string.IsNullOrWhiteSpace(introEn)) IntroEn = introEn;
            if (!string.IsNullOrWhiteSpace(contentEn)) ContentEn = contentEn;
            if (!string.IsNullOrWhiteSpace(conclusionEn)) ConclusionEn = conclusionEn;
            if (excerptEn != null) ExcerptEn = excerptEn;
            if (metaDescriptionEn != null) MetaDescriptionEn = metaDescriptionEn;
            if (metaKeywordsEn != null) MetaKeywordsEn = metaKeywordsEn;

            // سایر
            if (!string.IsNullOrWhiteSpace(slug)) Slug = slug;
            if (!string.IsNullOrWhiteSpace(thumbnailFile)) ThumbnailFile = thumbnailFile;
            if (authorId.HasValue) AuthorId = authorId.Value;

             MarkUpdated(currentUserId);
        }

        public void UpdateFile(int currentUserId,string? thumbnailFile = null)
        {
            if (!string.IsNullOrWhiteSpace(thumbnailFile)) ThumbnailFile = thumbnailFile;
            MarkUpdated(currentUserId);
        }

        // ===== Helpers =====
        private static string GenerateSlug(string title)
        {
            var slug = title.ToLower()
                .Replace(" ", "-")
                .Replace("آ", "a").Replace("ب", "b") // Persian to Latin
                                                     // ... سایر حروف
                ;
            return slug;
        }

        // Auto-generate Excerpt from Intro
        public string GetExcerptFa() =>
            !string.IsNullOrEmpty(ExcerptFa) ? ExcerptFa! :
            IntroFa.Length > 160 ? IntroFa[..160] + "..." : IntroFa;

        public string GetExcerptEn() =>
            !string.IsNullOrEmpty(ExcerptEn) ? ExcerptEn! :
            IntroEn.Length > 160 ? IntroEn[..160] + "..." : IntroEn;
    }
}