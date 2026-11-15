using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Domain.Entities;

namespace Infrastructure.Persistence.Configurations
{
    public class ProductOfferConfiguration : IEntityTypeConfiguration<ProductOffers>
    {
        public void Configure(EntityTypeBuilder<ProductOffers> builder)
        {
            builder.HasKey(po => po.Id);

            builder.Property(po => po.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(po => po.BasePrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(po => po.VariantName)
                   .HasMaxLength(ProductOffers.VariantMaxLength);

            builder.Property(po => po.VariantValue)
                   .HasMaxLength(ProductOffers.VariantMaxLength);

            // ===== Product =====
            builder.HasOne(po => po.Product)
                   .WithMany()
                   .HasForeignKey(po => po.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ===== Supplier =====
            builder.HasOne(po => po.Supplier)
                   .WithMany()
                   .HasForeignKey(po => po.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ===== Discounts (ProductOfferDiscount) =====
            builder.HasMany(po => po.Discounts)
                   .WithOne(pd => pd.ProductOffer)
                   .HasForeignKey(pd => pd.ProductOfferId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("ProductOffers");
        }
    }
}
