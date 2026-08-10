using Application.Common.Interfaces;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Repository;
using Infrastructure.Services;
using Infrastructure.Services.payment;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.Interfaces;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Jobs;
using OnlineShop.Infrastructure.Persistence;
using OnlineShop.Infrastructure.Persistence.Repositories;
using OnlineShop.Infrastructure.Repositories;
using OnlineShop.Infrastructure.Security;
using OnlineShop.Infrastructure.Services;
using Services.Services.Uploader;

namespace OnlineShop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                // EntityConfig HasData uses JsonSerializer.Serialize; output can differ across OS/runtime
                // vs the Windows-generated snapshot and falsely trip PendingModelChangesWarning.
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
            services.AddHangfire(config => config.UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IDiscountRepository, DiscountRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IProductOfferDiscountRepository, ProductOfferDiscountRepository>();
            services.AddScoped<IProductOfferRepository, ProductOfferRepository>();
            services.AddScoped<IProductOfferTagRepository, ProductOfferTagRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IEntityConfigRepository, EntityConfigRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ISlideRepository, SlideRepository>();
            services.AddScoped<ISpecialOfferRepository, SpecialOfferRepository>();
            services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
            services.AddScoped<IShippingMethodRepository, ShippingMethodRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IOrderJobService, OrderJobService>();
            services.AddScoped<HangfireOrderJob>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IUploaderService, UploaderService>();
            services.AddScoped<IBackupService, PostgresBackupService>();
            services.AddScoped<ISampleSeedService, SampleSeedService>();
            services.AddHttpClient<IPaymentGateway, ZarinpalPaymentGateway>();
            services.AddScoped<IProductSpecificationRepository, ProductSpecificationRepository>();
            services.AddScoped<IBlogTagRepository, BlogTagRepository>();
            services.AddScoped<IUserTagRepository, UserTagRepository>();
            services.AddScoped<ISeoSettingRepository, SeoSettingRepository>();
            services.AddScoped<IBlogContentQualityService, BlogContentQualityService>();
            services.AddHttpClient("SeoOpsProbe", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OnlineShop-SeoOps/1.0");
            });
            services.AddScoped<ISeoOpsService, SeoOpsService>();
            services.AddHttpContextAccessor();
            services.AddScoped<IDataInitializer, EntityConfigApiUrlNormalizer>();
            services.AddScoped<IDataInitializer, AdminUserInitializer>();
            services.AddScoped<IDataInitializer, ContentEditorUserInitializer>();
            return services;
        }
    }
}
