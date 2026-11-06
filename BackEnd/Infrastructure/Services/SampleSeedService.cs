using OnlineShop.Domain.Common;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace OnlineShop.Infrastructure.Services;

public class SampleSeedService : ISampleSeedService
{
    private readonly string _connectionString;
    private readonly string _seedsDirectory;
    private readonly ILogger<SampleSeedService> _logger;

    public SampleSeedService(IConfiguration configuration, ILogger<SampleSeedService> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new DependencyFailureException("رشتهٔ اتصال پایگاه داده در تنظیمات سرور تعریف نشده است.");

        var configured = configuration["Seed:SampleDirectory"];
        _seedsDirectory = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "seeds")
            : Path.GetFullPath(configured);
    }

    public Task<SampleSeedStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var files = ListSeedFiles()
            .Select(Path.GetFileName)
            .Where(f => f is not null)
            .Cast<string>()
            .ToList();

        var dto = new SampleSeedStatusDto(
            SeedsAvailable: files.Count > 0,
            SeedsDirectory: _seedsDirectory,
            Files: files,
            AutoSeedNote:
                "On startup: Roles + EntityConfig (EF migration) and optional admin user (Seed:AdminPassword). " +
                "Sample catalog SQL is applied only from this admin action or database/seed.sh.");

        return Task.FromResult(dto);
    }

    public async Task<SampleSeedResultDto> ApplyAsync(bool clean, CancellationToken cancellationToken = default)
    {
        var files = ListSeedFiles();
        if (files.Count == 0)
        {
            return new SampleSeedResultDto(
                false,
                $"هیچ فایل SQL نمونه‌ای در مسیر «{_seedsDirectory}» پیدا نشد.",
                Array.Empty<string>());
        }

        var applied = new List<string>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (clean)
        {
            var truncate = Path.Combine(_seedsDirectory, "00_truncate_sample_data.sql");
            if (File.Exists(truncate))
            {
                await ExecuteSqlFileAsync(connection, truncate, cancellationToken);
                applied.Add(Path.GetFileName(truncate)!);
            }
        }

        foreach (var file in files)
        {
            var name = Path.GetFileName(file)!;
            if (name.Equals("00_truncate_sample_data.sql", StringComparison.OrdinalIgnoreCase))
                continue;

            await ExecuteSqlFileAsync(connection, file, cancellationToken);
            applied.Add(name);
        }

        _logger.LogInformation("Sample seed applied ({Count} files, clean={Clean})", applied.Count, clean);

        return new SampleSeedResultDto(
            true,
            clean ? "Sample data cleaned and re-seeded." : "Sample data seeded.",
            applied);
    }

    private List<string> ListSeedFiles()
    {
        if (!Directory.Exists(_seedsDirectory))
            return new List<string>();

        return Directory
            .EnumerateFiles(_seedsDirectory, "*.sql")
            .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task ExecuteSqlFileAsync(
        NpgsqlConnection connection,
        string filePath,
        CancellationToken cancellationToken)
    {
        var sql = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(sql))
            return;

        await using var cmd = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 120
        };
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
