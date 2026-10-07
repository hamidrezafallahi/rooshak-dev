using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Configurations
{
    public class ThemeSettingConfiguration : IEntityTypeConfiguration<ThemeSetting>
    {
        public void Configure(EntityTypeBuilder<ThemeSetting> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

            foreach (var name in new[]
                     {
                         nameof(ThemeSetting.PrimaryColor), nameof(ThemeSetting.SecondaryColor),
                         nameof(ThemeSetting.HighlightColor), nameof(ThemeSetting.NeutralColor),
                         nameof(ThemeSetting.SuccessColor), nameof(ThemeSetting.ErrorColor),
                         nameof(ThemeSetting.WarningColor), nameof(ThemeSetting.InfoColor),
                         nameof(ThemeSetting.SurfaceColor), nameof(ThemeSetting.SurfaceMutedColor),
                         nameof(ThemeSetting.BorderColor), nameof(ThemeSetting.TextColor),
                         nameof(ThemeSetting.TextMutedColor),
                     })
            {
                builder.Property(name).IsRequired().HasMaxLength(7);
            }

            builder.ToTable("ThemeSettings");

            // تم پیش‌فرض (همان پالت مونوکروم فعلی سایت) تا سایت از روز اول رنگ‌هایش را از دیتابیس بگیرد.
            builder.HasData(new
            {
                Id = 1,
                Name = "Maison Crystal",
                PrimaryColor = "#000000",
                SecondaryColor = "#f2f2f2",
                HighlightColor = "#a38a52",
                NeutralColor = "#f7f7f7",
                SuccessColor = "#0d9488",
                ErrorColor = "#c8102e",
                WarningColor = "#b45309",
                InfoColor = "#3b82f6",
                SurfaceColor = "#ffffff",
                SurfaceMutedColor = "#f7f7f7",
                BorderColor = "#e3e3e3",
                TextColor = "#000000",
                TextMutedColor = "#6b6b6b",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = 1,
            });
        }
    }
}
