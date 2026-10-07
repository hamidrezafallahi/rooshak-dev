using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Configurations
{
    public class SlideConfiguration : IEntityTypeConfiguration<Slide>
    {
        public void Configure(EntityTypeBuilder<Slide> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(b => b.BannerUrl)
                   .HasMaxLength(200);
            builder.Property(b => b.BannerTitle)
                   .HasMaxLength(200);
            builder.Property(b => b.BannerDescription)
                   .HasMaxLength(400);
            builder.Property(b => b.FirstUrl)
                   .HasMaxLength(200);
            builder.Property(b => b.SecondUrl)
                   .HasMaxLength(200);
            builder.Property(b => b.VideoUrl)
                   .HasMaxLength(300);
            builder.Property(ua => ua.IsHero)
                    .IsRequired()
                    .HasDefaultValue(false);
        }
    }
}
