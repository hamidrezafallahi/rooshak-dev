using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Configurations
{
    public class AnnouncementBarConfiguration : IEntityTypeConfiguration<AnnouncementBar>
    {
        public void Configure(EntityTypeBuilder<AnnouncementBar> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageFa).IsRequired().HasMaxLength(300);
            builder.Property(x => x.MessageEn).IsRequired().HasMaxLength(300);
            builder.Property(x => x.LinkUrl).IsRequired().HasMaxLength(500);
            builder.Property(x => x.BackgroundImageUrl).IsRequired().HasMaxLength(300);
            builder.Property(x => x.BackgroundColor).IsRequired().HasMaxLength(7);
            builder.Property(x => x.TextColor).IsRequired().HasMaxLength(7);
            builder.Property(x => x.HeightPx).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();

            builder.HasIndex(x => new { x.IsActive, x.StartsAt, x.EndsAt });

            builder.ToTable("AnnouncementBars");

            // همان پیام فعلی سایت، تا بعد از مهاجرت نوار از بین نرود.
            builder.HasData(new
            {
                Id = 1,
                MessageFa = "بسته‌بندی ایمن و ضدضربه برای تمام ظروف کریستال · ارسال به سراسر ایران",
                MessageEn = "Complimentary gift wrapping · Secure packing for every crystal piece",
                LinkUrl = "",
                BackgroundImageUrl = "",
                BackgroundColor = "#000000",
                TextColor = "#ffffff",
                HeightPx = 36,
                DisplayOrder = 0,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = 1,
            });
        }
    }
}
