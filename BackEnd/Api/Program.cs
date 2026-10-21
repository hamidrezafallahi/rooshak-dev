using Api.Security;
using Application.DataTransfer.Excel;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OnlineShop.Infrastructure;
using OnlineShop.Infrastructure.Persistence;
using System.Text;
using System.Threading.RateLimiting;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Admin forms historically send numeric inputs as JSON strings.
                options.JsonSerializerOptions.NumberHandling =
                    System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            });
        builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 52_428_800; // 50 MB
            options.ValueLengthLimit = 52_428_800;
            options.MemoryBufferThreshold = 1024 * 1024;
        });
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 52_428_800;
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "لطفاً توکن JWT را اینجا وارد کنید. مثال: Bearer {token}"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
        });
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddExcelDataTransfer();
        builder.Services.AddHealthChecks();
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;

            // Only nginx and the Next.js server (both inside Docker) may say who the
            // client is. From anywhere else X-Forwarded-For is whatever the caller sent,
            // and believing it would let one client spread itself over many rate-limit
            // buckets or pass as an internal caller.
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var network in InternalNetworks.All)
                options.KnownIPNetworks.Add(network);
        });
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name.Contains("Application"))
            .ToArray();
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });
        var internalPermitLimit = builder.Configuration.GetValue("RateLimiting:InternalPermitLimit", 1000);
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                // After UseForwardedHeaders this is the visitor's IP, also for the calls
                // the Next.js server makes for a visitor (BFF, login, token refresh).
                var address = context.Connection.RemoteIpAddress;
                // Still an internal address: a call from inside Docker with no visitor
                // behind it (storefront ISR fetches, n8n, health checks). Its bucket is
                // larger, as page renders for every crawler and visitor land in it.
                var isInternal = address is not null && InternalNetworks.Contains(address);
                var ip = address?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = isInternal ? internalPermitLimit : 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 5
                });
            });
        });

        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key must be set via environment/secrets and be at least 32 characters.");

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            })
            .AddScheme<AuthenticationSchemeOptions, CatalogApiKeyAuthenticationHandler>(
                Application.Common.CatalogApiKey.SchemeName,
                _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("StoreManagerOnly", policy => policy.RequireRole("StoreManager"));
            options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
            options.AddPolicy(Application.Common.CatalogApiKey.PolicyName, policy =>
            {
                policy.AddAuthenticationSchemes("Bearer", Application.Common.CatalogApiKey.SchemeName);
                policy.RequireAuthenticatedUser();
                policy.RequireRole("SuperAdmin", "Admin", "ContentEditor");
            });
        });
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });

        var app = builder.Build();
        app.UseForwardedHeaders();

        var swaggerEnabled = app.Environment.IsDevelopment() ||
            builder.Configuration.GetValue<bool>("Swagger:Enabled");

        if (swaggerEnabled)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (allowedOrigins.Length > 0)
        {
            app.UseCors("AllowFrontend");
        }

        app.InitializeDatabase();
        // ASP.NET refuses to serve unknown extensions: register the 3D formats (GLB/glTF/USDZ).
        var staticContentTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        staticContentTypes.Mappings[".glb"] = "model/gltf-binary";
        staticContentTypes.Mappings[".gltf"] = "model/gltf+json";
        staticContentTypes.Mappings[".usdz"] = "model/vnd.usdz+zip";
        app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticContentTypes });
        app.UseMiddleware<BlacklistMiddleware>();
        app.UseMiddleware<WhitelistMiddleware>();
        app.UseMiddleware<SuspiciousClientMiddleware>();
        app.UseRateLimiter();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        try
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[]
                {
                    new HangfireDashboardAuthFilter(app.Environment, app.Configuration)
                }
            });
        }
        catch
        {
            // Hangfire dashboard unavailable if database not configured
        }
        app.MapHealthChecks("/health");
        app.MapControllers();
        app.Run();
    }
}
