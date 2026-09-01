using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OnlineShop.Infrastructure.Persistence;

/// <summary>
/// Applies EF migrations (with legacy-DB baseline support) and runs <see cref="IDataInitializer"/> seeds at startup.
/// </summary>
public static class DatabaseInitializerExtensions
{
    public static IApplicationBuilder InitializeDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetService<ILoggerFactory>()?
            .CreateLogger("DatabaseInitializer");

        if (dbContext is null)
            return app;

        try
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
            if (pendingMigrations.Count > 0)
            {
                // Legacy DBs may have been created with EnsureCreated (no __EFMigrationsHistory).
                // Baseline InitialCreate as applied, then run the remaining additive migrations.
                var appliedAny = dbContext.Database.GetAppliedMigrations().Any();
                var hasLegacySchema = !appliedAny && TableExists(dbContext, "EntityConfigs");
                if (hasLegacySchema)
                {
                    logger?.LogWarning(
                        "Legacy DB without EF migration history detected. Baselining {MigrationId}, then applying pending migrations.",
                        "20260729110612_InitialCreate");
                    BaselineInitialMigration(dbContext, "20260729110612_InitialCreate");
                }

                dbContext.Database.Migrate();
            }
            else
            {
                dbContext.Database.EnsureCreated();
            }

            foreach (var dataInitializer in scope.ServiceProvider.GetServices<IDataInitializer>())
            {
                try
                {
                    dataInitializer.InitializeData();
                }
                catch (Exception ex)
                {
                    logger?.LogError(
                        ex,
                        "Data initializer {Initializer} failed. Application will continue.",
                        dataInitializer.GetType().Name);
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(
                ex,
                "Database initialization failed. The API will run without database access. " +
                "This is acceptable for testing/development without a configured database.");
        }

        return app;
    }

    private static void BaselineInitialMigration(AppDbContext dbContext, string migrationId)
    {
        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT {0}, {1}
            WHERE NOT EXISTS (
                SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = {0}
            );
            """,
            migrationId,
            "10.0.0");
    }

    private static bool TableExists(AppDbContext dbContext, string tableName)
    {
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = @tableName
                );
                """;
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();
            if (shouldClose)
                connection.Close();

            return result is true || result is bool b && b;
        }
        catch
        {
            return false;
        }
    }
}
