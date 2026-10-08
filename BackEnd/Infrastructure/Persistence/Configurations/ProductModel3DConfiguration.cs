using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence.Configurations
{
    public class ProductModel3DConfiguration : IEntityTypeConfiguration<ProductModel3D>
    {
        public void Configure(EntityTypeBuilder<ProductModel3D> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();

            builder.Property(m => m.ModelUrl).IsRequired().HasMaxLength(500);
            builder.Property(m => m.UsdzUrl).HasMaxLength(500);

            builder.HasOne(m => m.Product)
                   .WithMany()
                   .HasForeignKey(m => m.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One live model per product (soft-deleted rows are ignored).
            builder.HasIndex(m => m.ProductId)
                   .IsUnique()
                   .HasFilter("\"IsDeleted\" = false");
        }
    }

    public class ProductScanSourceConfiguration : IEntityTypeConfiguration<ProductScanSource>
    {
        public void Configure(EntityTypeBuilder<ProductScanSource> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.FileUrl).IsRequired().HasMaxLength(500);
            builder.Property(s => s.Kind).IsRequired().HasMaxLength(10);

            builder.HasOne(s => s.Product)
                   .WithMany()
                   .HasForeignKey(s => s.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.ProductId);
        }
    }
}
