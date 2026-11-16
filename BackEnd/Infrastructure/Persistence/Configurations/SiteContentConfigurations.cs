using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Configurations
{
    public class HomeSlideConfiguration : IEntityTypeConfiguration<HomeSlide>
    {
        public void Configure(EntityTypeBuilder<HomeSlide> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(300);
            builder.Property(x => x.MobileImageUrl).IsRequired().HasMaxLength(300);
            builder.Property(x => x.LinkUrl).IsRequired().HasMaxLength(HomeSlide.MaxUrlLength);
            builder.Property(x => x.TitleFa).IsRequired().HasMaxLength(HomeSlide.MaxTitleLength);
            builder.Property(x => x.TitleEn).IsRequired().HasMaxLength(HomeSlide.MaxTitleLength);
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });
            builder.ToTable("HomeSlides");
        }
    }

    public class CustomFontConfiguration : IEntityTypeConfiguration<CustomFont>
    {
        public void Configure(EntityTypeBuilder<CustomFont> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Ignore(x => x.Key);
            builder.Property(x => x.NameFa).IsRequired().HasMaxLength(CustomFont.MaxNameLength);
            builder.Property(x => x.NameEn).IsRequired().HasMaxLength(CustomFont.MaxNameLength);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(20);
            builder.Property(x => x.FileUrl).IsRequired().HasMaxLength(300);
            builder.ToTable("CustomFonts");
        }
    }

    public class AboutSectionConfiguration : IEntityTypeConfiguration<AboutSection>
    {
        public void Configure(EntityTypeBuilder<AboutSection> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SectionKey).IsRequired().HasMaxLength(AboutSection.MaxKeyLength);
            builder.Property(x => x.EyebrowFa).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.EyebrowEn).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.TitleFa).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.TitleEn).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.BodyFa).IsRequired().HasMaxLength(AboutSection.MaxBodyLength);
            builder.Property(x => x.BodyEn).IsRequired().HasMaxLength(AboutSection.MaxBodyLength);
            builder.Property(x => x.LinkTextFa).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.LinkTextEn).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.Property(x => x.LinkUrl).IsRequired().HasMaxLength(AboutSection.MaxShortLength);
            builder.HasIndex(x => x.SectionKey).IsUnique().HasFilter("\"IsDeleted\" = false");
            builder.ToTable("AboutSections");

            // متن اولیه‌ی صفحه‌ی «درباره ما»؛ ادمین همه را از پنل (درباره ما) ویرایش می‌کند.
            // ادعایی مثل «اصل» یا «دست‌ساز» عمداً نوشته نشده؛ نقل‌قول و آمارها خالی‌اند تا ادمین پرشان کند.
            builder.HasData(
                Row(1, "hero", 1,
                    "درباره ما", "About us",
                    "درباره روشاک", "About Rooshak",
                    "روشاک فروشگاه آنلاین ظروف کریستال و دکوری برای پذیرایی، هدیه و جهیزیه است.",
                    "Rooshak is an online store for crystal and decorative tableware for hosting, gifts and the wedding trousseau."),
                Row(2, "intro", 2,
                    "", "",
                    "ما چه می‌کنیم", "What we do",
                    "ظروف را با دقت انتخاب می‌کنیم، مشخصات هر محصول را روشن می‌نویسیم و تلاش می‌کنیم خرید آنلاین ظروف شکستنی ساده و مطمئن باشد.",
                    "We choose each piece with care, describe every product clearly and work to make buying fragile tableware online simple and safe."),
                Row(3, "packaging", 3,
                    "", "",
                    "بسته‌بندی و ارسال", "Packaging and delivery",
                    "ظروف کریستال شکستنی‌اند؛ هر سفارش با بسته‌بندی محافظ آماده و ارسال می‌شود.",
                    "Crystal is fragile, so every order is packed with protective packaging before it ships."),
                Row(4, "support", 4,
                    "", "",
                    "مشاوره پیش از خرید", "Help before you buy",
                    "برای انتخاب ست پذیرایی، هدیه یا جهیزیه می‌توانید با ما در تماس باشید.",
                    "Get in touch if you need help choosing a serving set, a gift or pieces for a trousseau.",
                    "تماس با ما", "Contact us", "cooperation"),
                Row(5, "quote", 5, "", "", "", "", "", ""),
                Row(6, "fact-1", 6, "", "", "", "", "", ""),
                Row(7, "fact-2", 7, "", "", "", "", "", ""),
                Row(8, "fact-3", 8, "", "", "", "", "", ""),
                Row(9, "cta", 9, "", "", "سؤالی دارید؟ با ما در تماس باشید.", "Questions? Get in touch.", "", "",
                    "تماس با ما", "Contact us", "cooperation"));
        }

        private static object Row(
            int id, string key, int order,
            string eyebrowFa, string eyebrowEn,
            string titleFa, string titleEn,
            string bodyFa, string bodyEn,
            string linkTextFa = "", string linkTextEn = "", string linkUrl = "") => new
            {
                Id = id,
                SectionKey = key,
                DisplayOrder = order,
                EyebrowFa = eyebrowFa,
                EyebrowEn = eyebrowEn,
                TitleFa = titleFa,
                TitleEn = titleEn,
                BodyFa = bodyFa,
                BodyEn = bodyEn,
                LinkTextFa = linkTextFa,
                LinkTextEn = linkTextEn,
                LinkUrl = linkUrl,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = 1,
            };
    }
}
