using Application.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OnlineShop.Domain.Common;
using OnlineShop.Domain.Entities;

namespace OnlineShop.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserAddress> Addresses => Set<UserAddress>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductModel3D> ProductModels3D => Set<ProductModel3D>();
        public DbSet<ProductScanSource> ProductScanSources => Set<ProductScanSource>();
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<BlogTag> BlogTags => Set<BlogTag>();
        public DbSet<UserTag> UserTags => Set<UserTag>();
        public DbSet<ProductOfferTag> ProductOfferTags => Set<ProductOfferTag>();
        public DbSet<EntityConfig> EntityConfigs => Set<EntityConfig>();
        public DbSet<Slide> Slides => Set<Slide>();
        public DbSet<SpecialOffer> SpecialOffers => Set<SpecialOffer>();
        public DbSet<DiscountCode> DiscountCode => Set<DiscountCode>();
        public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
        public DbSet<PaymentMethod> PaymentMethod => Set<PaymentMethod>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Rate> Rates => Set<Rate>();
        public DbSet<ProductSpecification> ProductSpecification => Set<ProductSpecification>();
        public DbSet<ProductOffers> ProductOffers => Set<ProductOffers>();
        public DbSet<SeoSetting> SeoSettings => Set<SeoSetting>();
        public DbSet<Faq> Faqs => Set<Faq>();
        public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();
        public DbSet<ThemeSetting> ThemeSettings => Set<ThemeSetting>();
        public DbSet<AnnouncementBar> AnnouncementBars => Set<AnnouncementBar>();




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            EnsureStringLengths();
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            catch (Exception ex) when (ex is DbUpdateException or NpgsqlException)
            {
                throw DatabaseErrorTranslator.Translate(ex, Model) ?? ex;
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            EnsureStringLengths();
            try
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            catch (Exception ex) when (ex is DbUpdateException or NpgsqlException)
            {
                throw DatabaseErrorTranslator.Translate(ex, Model) ?? ex;
            }
        }

        // PostgreSQL rejects an over-length varchar with a bare 22001 error that names no
        // column, so check against the model's HasMaxLength values first and report the fields.
        private void EnsureStringLengths()
        {
            var violations = new List<FieldLengthViolation>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified))
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue is not string value)
                        continue;
                    if (entry.State == EntityState.Modified && !property.IsModified)
                        continue;

                    var maxLength = property.Metadata.GetMaxLength();
                    if (maxLength.HasValue && value.Length > maxLength.Value)
                    {
                        var name = property.Metadata.Name;
                        violations.Add(new FieldLengthViolation(
                            char.ToLowerInvariant(name[0]) + name[1..],
                            maxLength.Value,
                            value.Length,
                            FieldLabels.Field(name)));
                    }
                }
            }

            if (violations.Count > 0)
                throw new FieldLengthExceededException(violations);
        }
    }
}
