using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace OnlineShop.Infrastructure.Services
{
    public class PostgresBackupService : IBackupService
    {
        private const string AutoSuffix = "_auto";
        private const string PreRestoreSuffix = "_pre";
        private const int PreRestoreKeepCount = 3;

        // Matches every file this service creates (and nothing else) so listing/download/delete stay safe.
        private static readonly Regex SafeFileNameRegex = new(
            @"^(?:backup_\d{8}_\d{6}\.sql|full_\d{8}_\d{6}(?:_auto|_pre)?\.zip)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private const string DropAllSchemasSql = """
            DO $$ DECLARE s text; BEGIN
              FOR s IN SELECT nspname FROM pg_namespace
                       WHERE nspname <> 'information_schema' AND nspname NOT LIKE 'pg\_%' LOOP
                EXECUTE format('DROP SCHEMA %I CASCADE', s);
              END LOOP;
              CREATE SCHEMA public;
            END $$;

            """;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        // Backup, restore and scheduled runs are mutually exclusive (the service is a singleton).
        private static readonly SemaphoreSlim Gate = new(1, 1);
        private static readonly object SettingsLock = new();

        private readonly string _connectionString;
        private readonly string _backupDirectory;
        private readonly string _pgDumpPath;
        private readonly string _pgRestorePath;
        private readonly string _psqlPath;
        private readonly IWebHostEnvironment _webHost;
        private readonly ILogger<PostgresBackupService> _logger;
        private DateTime? _lastScheduledAttemptUtc;

        public PostgresBackupService(
            IConfiguration configuration,
            IWebHostEnvironment webHost,
            ILogger<PostgresBackupService> logger)
        {
            _logger = logger;
            _webHost = webHost;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            var configuredDir = configuration["Backup:Directory"];
            _backupDirectory = string.IsNullOrWhiteSpace(configuredDir)
                ? Path.Combine(AppContext.BaseDirectory, "backups")
                : Path.GetFullPath(configuredDir);

            _pgDumpPath = configuration["Backup:PgDumpPath"] ?? "pg_dump";
            _pgRestorePath = configuration["Backup:PgRestorePath"] ?? "pg_restore";
            _psqlPath = configuration["Backup:PsqlPath"] ?? "psql";

            Directory.CreateDirectory(_backupDirectory);
        }

        // ── legacy: plain SQL dump ────────────────────────────────────────────

        public async Task<BackupFileDto> CreateAsync(CancellationToken cancellationToken = default)
        {
            var settings = ParseConnectionString(_connectionString);
            var fileName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql";
            var filePath = Path.Combine(_backupDirectory, fileName);

            var args = new StringBuilder();
            args.Append($"--host={EscapeArg(settings.Host)} ");
            args.Append($"--port={settings.Port} ");
            args.Append($"--username={EscapeArg(settings.Username)} ");
            args.Append($"--dbname={EscapeArg(settings.Database)} ");
            args.Append("--format=plain ");
            args.Append("--no-owner ");
            args.Append("--no-acl ");
            args.Append($"--file={EscapeArg(filePath)}");

            var startInfo = new ProcessStartInfo
            {
                FileName = _pgDumpPath,
                Arguments = args.ToString(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.Environment["PGPASSWORD"] = settings.Password;

            using var process = new Process { StartInfo = startInfo };

            try
            {
                if (!process.Start())
                    throw new InvalidOperationException("Failed to start pg_dump process.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "pg_dump was not found. Install PostgreSQL client tools or set Backup:PgDumpPath.",
                    ex);
            }

            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            var stderr = await stderrTask;
            _ = await stdoutTask;

            if (process.ExitCode != 0)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                _logger.LogError("pg_dump failed with exit code {ExitCode}: {Error}", process.ExitCode, stderr);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(stderr)
                        ? $"pg_dump failed with exit code {process.ExitCode}."
                        : stderr.Trim());
            }

            return ToDto(new FileInfo(filePath));
        }

        // ── listing / download / delete ───────────────────────────────────────

        public Task<IReadOnlyList<BackupFileDto>> ListAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = Directory.EnumerateFiles(_backupDirectory)
                .Select(path => new FileInfo(path))
                .Where(info => SafeFileNameRegex.IsMatch(info.Name))
                .OrderByDescending(info => info.CreationTimeUtc)
                .Select(ToDto)
                .ToList();

            return Task.FromResult<IReadOnlyList<BackupFileDto>>(items);
        }

        public Task<(Stream Stream, string ContentType, string FileName)?> OpenDownloadAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryResolveSafePath(fileName, out var filePath) || !File.Exists(filePath))
                return Task.FromResult<(Stream, string, string)?>(null);

            Stream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            var contentType = filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? "application/zip"
                : "application/sql";

            return Task.FromResult<(Stream, string, string)?>(
                (stream, contentType, Path.GetFileName(filePath)));
        }

        public Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryResolveSafePath(fileName, out var filePath) || !File.Exists(filePath))
                return Task.FromResult(false);

            File.Delete(filePath);
            return Task.FromResult(true);
        }

        // ── full backup (database + uploads) ──────────────────────────────────

        public async Task<BackupFileDto> CreateFullAsync(CancellationToken cancellationToken = default)
        {
            await Gate.WaitAsync(cancellationToken);
            try
            {
                return await CreateFullCoreAsync(string.Empty, cancellationToken);
            }
            finally
            {
                Gate.Release();
            }
        }

        private async Task<BackupFileDto> CreateFullCoreAsync(string suffix, CancellationToken ct)
        {
            var connection = ParseConnectionString(_connectionString);
            var workDir = NewWorkDirectory();
            var partPath = Path.Combine(workDir, "backup.zip.part");

            try
            {
                var dumpPath = Path.Combine(workDir, "database.dump");
                await RunToolAsync(_pgDumpPath, connection, ct, null,
                    $"--host={connection.Host}",
                    $"--port={connection.Port}",
                    $"--username={connection.Username}",
                    $"--dbname={connection.Database}",
                    "--format=custom",
                    "--no-owner",
                    "--no-acl",
                    $"--file={dumpPath}");

                var dumpSha = await Sha256Async(dumpPath, ct);
                var uploadsRoot = GetUploadsRoot();
                var uploadFiles = Directory.Exists(uploadsRoot)
                    ? Directory.EnumerateFiles(uploadsRoot, "*", SearchOption.AllDirectories).ToList()
                    : new List<string>();

                var manifest = new BackupManifest
                {
                    Format = 1,
                    CreatedAtUtc = DateTime.UtcNow,
                    Database = connection.Database,
                    DumpSha256 = dumpSha,
                    UploadsFileCount = uploadFiles.Count,
                    AppVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown",
                };

                using (var fs = new FileStream(partPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    var manifestEntry = zip.CreateEntry("manifest.json", CompressionLevel.Optimal);
                    await using (var manifestStream = manifestEntry.Open())
                        await JsonSerializer.SerializeAsync(manifestStream, manifest, JsonOptions, ct);

                    // pg_dump -Fc output is already compressed.
                    var dumpEntry = zip.CreateEntry("database.dump", CompressionLevel.NoCompression);
                    await using (var entryStream = dumpEntry.Open())
                    await using (var dumpSource = File.OpenRead(dumpPath))
                        await dumpSource.CopyToAsync(entryStream, ct);

                    foreach (var file in uploadFiles)
                    {
                        FileStream source;
                        try
                        {
                            // A file being written by an upload right now must not break the whole backup.
                            source = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            _logger.LogWarning(ex, "Skipping unreadable upload {File} during backup", file);
                            continue;
                        }

                        await using (source)
                        {
                            var relative = Path.GetRelativePath(uploadsRoot, file).Replace('\\', '/');
                            var entry = zip.CreateEntry($"uploads/{relative}", CompressionLevel.Fastest);
                            await using var entryStream = entry.Open();
                            await source.CopyToAsync(entryStream, ct);
                        }
                    }
                }

                // Names have one-second resolution; never overwrite a backup created in the same second.
                string finalPath;
                while (true)
                {
                    finalPath = Path.Combine(_backupDirectory, $"full_{DateTime.UtcNow:yyyyMMdd_HHmmss}{suffix}.zip");
                    if (!File.Exists(finalPath))
                        break;
                    await Task.Delay(1100, ct);
                }

                File.Move(partPath, finalPath, overwrite: false);
                return ToDto(new FileInfo(finalPath));
            }
            finally
            {
                TryDeleteDirectory(workDir);
            }
        }

        public async Task<RestoreResultDto> RestoreFullAsync(Stream zipStream, CancellationToken cancellationToken = default)
        {
            await Gate.WaitAsync(cancellationToken);
            var workDir = NewWorkDirectory();
            try
            {
                var uploadedZip = Path.Combine(workDir, "upload.zip");
                await using (var target = new FileStream(uploadedZip, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                    await zipStream.CopyToAsync(target, cancellationToken);

                // 1) Validate and unpack everything into the work dir. Nothing live is touched yet.
                var dumpPath = Path.Combine(workDir, "database.dump");
                var newUploads = Path.Combine(workDir, "uploads-new");
                Directory.CreateDirectory(newUploads);
                var restoredFiles = await UnpackAndValidateAsync(uploadedZip, dumpPath, newUploads, cancellationToken);

                // 2) Safety net: snapshot the current state so a bad restore can itself be undone.
                var safety = await CreateFullCoreAsync(PreRestoreSuffix, cancellationToken);

                // 3) Database first. The dump is turned into a SQL script that first drops every schema
                //    (so tables added by newer migrations don't linger) and then recreates the backup's
                //    content, all inside ONE transaction: on any failure the current data is left intact.
                var connection = ParseConnectionString(_connectionString);
                var bodyPath = Path.Combine(workDir, "restore-body.sql");
                var scriptPath = Path.Combine(workDir, "restore.sql");
                await RunToolAsync(_pgRestorePath, connection, cancellationToken, null,
                    "--no-owner", "--no-acl", $"--file={bodyPath}", dumpPath);

                await using (var script = new FileStream(scriptPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await script.WriteAsync(Encoding.UTF8.GetBytes(DropAllSchemasSql), cancellationToken);
                    await using var body = File.OpenRead(bodyPath);
                    await body.CopyToAsync(script, cancellationToken);
                }

                await RunToolAsync(_psqlPath, connection, cancellationToken,
                    new Dictionary<string, string> { ["PGOPTIONS"] = "-c lock_timeout=120000" },
                    $"--host={connection.Host}",
                    $"--port={connection.Port}",
                    $"--username={connection.Username}",
                    $"--dbname={connection.Database}",
                    "--set=ON_ERROR_STOP=1",
                    "--single-transaction",
                    "--quiet",
                    $"--file={scriptPath}");
                NpgsqlConnection.ClearAllPools();

                // 4) Files.
                ReplaceUploads(newUploads);

                PruneByKind(PreRestoreSuffix + ".zip", PreRestoreKeepCount);
                _logger.LogWarning("Full restore completed; safety backup {Safety}", safety.FileName);

                return new RestoreResultDto
                {
                    SafetyBackupFileName = safety.FileName,
                    RestoredFileCount = restoredFiles,
                };
            }
            finally
            {
                TryDeleteDirectory(workDir);
                Gate.Release();
            }
        }

        private static async Task<int> UnpackAndValidateAsync(
            string zipPath,
            string dumpTargetPath,
            string uploadsTargetDir,
            CancellationToken ct)
        {
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(zipPath);
            }
            catch (InvalidDataException)
            {
                throw new InvalidOperationException("فایل انتخاب‌شده یک بک‌آپ معتبر (zip) نیست.");
            }

            using (archive)
            {
                var manifestEntry = archive.GetEntry("manifest.json")
                    ?? throw new InvalidOperationException("این فایل بک‌آپ کامل نیست (manifest.json یافت نشد).");

                BackupManifest? manifest;
                await using (var manifestStream = manifestEntry.Open())
                    manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(manifestStream, JsonOptions, ct);

                if (manifest is null || manifest.Format != 1)
                    throw new InvalidOperationException("نسخه‌ی فرمت این بک‌آپ پشتیبانی نمی‌شود.");

                var dumpEntry = archive.GetEntry("database.dump")
                    ?? throw new InvalidOperationException("فایل دیتابیس داخل بک‌آپ یافت نشد.");

                using (var sha = SHA256.Create())
                {
                    await using (var entryStream = dumpEntry.Open())
                    await using (var output = new FileStream(dumpTargetPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                    await using (var crypto = new CryptoStream(output, sha, CryptoStreamMode.Write))
                        await entryStream.CopyToAsync(crypto, ct);

                    var actual = Convert.ToHexString(sha.Hash!);
                    if (!string.Equals(actual, manifest.DumpSha256, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("بک‌آپ آسیب دیده است (checksum دیتابیس مطابقت ندارد).");
                }

                var rootFull = Path.GetFullPath(uploadsTargetDir)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;
                var count = 0;

                foreach (var entry in archive.Entries)
                {
                    const string prefix = "uploads/";
                    if (!entry.FullName.StartsWith(prefix, StringComparison.Ordinal) || entry.FullName.EndsWith('/'))
                        continue;

                    var destination = Path.GetFullPath(Path.Combine(uploadsTargetDir, entry.FullName[prefix.Length..]));
                    if (!destination.StartsWith(rootFull, StringComparison.Ordinal))
                        throw new InvalidOperationException("بک‌آپ شامل مسیر نامعتبر است و پذیرفته نشد.");

                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    entry.ExtractToFile(destination, overwrite: true);
                    count++;
                }

                return count;
            }
        }

        private void ReplaceUploads(string newUploadsDir)
        {
            var root = GetUploadsRoot();
            Directory.CreateDirectory(root);

            // The uploads folder itself is a mounted volume, so only its contents can be replaced.
            foreach (var file in Directory.EnumerateFiles(root))
                File.Delete(file);
            foreach (var dir in Directory.EnumerateDirectories(root))
                Directory.Delete(dir, recursive: true);

            CopyDirectory(newUploadsDir, root);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var file in Directory.EnumerateFiles(source))
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
            foreach (var dir in Directory.EnumerateDirectories(source))
                CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
        }

        // ── scheduled backups & settings ──────────────────────────────────────

        public async Task<bool> RunScheduledIfDueAsync(CancellationToken cancellationToken = default)
        {
            var settings = LoadSettings();
            if (!settings.AutoEnabled)
                return false;

            var now = DateTime.UtcNow;
            var dueAt = now.Date.AddHours(settings.HourUtc);
            if (now < dueAt)
                return false;
            if (settings.LastAutoBackupUtc is { } last && last >= dueAt)
                return false;
            // After a failure, wait before retrying instead of retrying on every tick.
            if (_lastScheduledAttemptUtc is { } attempt && now - attempt < TimeSpan.FromHours(1))
                return false;

            _lastScheduledAttemptUtc = now;

            await Gate.WaitAsync(cancellationToken);
            try
            {
                await CreateFullCoreAsync(AutoSuffix, cancellationToken);
            }
            finally
            {
                Gate.Release();
            }

            lock (SettingsLock)
            {
                var fresh = LoadSettings();
                fresh.LastAutoBackupUtc = DateTime.UtcNow;
                WriteSettings(fresh);
            }

            PruneByKind(AutoSuffix + ".zip", settings.KeepCount);
            return true;
        }

        public Task<BackupSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(LoadSettings());

        public Task<BackupSettingsDto> SaveSettingsAsync(BackupSettingsDto settings, CancellationToken cancellationToken = default)
        {
            lock (SettingsLock)
            {
                var current = LoadSettings();
                current.AutoEnabled = settings.AutoEnabled;
                current.HourUtc = Math.Clamp(settings.HourUtc, 0, 23);
                current.KeepCount = Math.Clamp(settings.KeepCount, 1, 60);
                WriteSettings(current);
                return Task.FromResult(current);
            }
        }

        public Task MarkDownloadedAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (!SafeFileNameRegex.IsMatch(Path.GetFileName(fileName ?? string.Empty)) ||
                !fileName!.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            lock (SettingsLock)
            {
                var current = LoadSettings();
                current.LastDownloadedUtc = DateTime.UtcNow;
                WriteSettings(current);
            }
            return Task.CompletedTask;
        }

        private string SettingsPath => Path.Combine(_backupDirectory, "backup-settings.json");

        private BackupSettingsDto LoadSettings()
        {
            lock (SettingsLock)
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        var loaded = JsonSerializer.Deserialize<BackupSettingsDto>(File.ReadAllText(SettingsPath), JsonOptions);
                        if (loaded is not null)
                            return loaded;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read backup settings; using defaults");
                }

                return new BackupSettingsDto();
            }
        }

        private void WriteSettings(BackupSettingsDto settings)
        {
            var temp = SettingsPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(settings, JsonOptions));
            File.Move(temp, SettingsPath, overwrite: true);
        }

        private void PruneByKind(string nameEnding, int keep)
        {
            try
            {
                var stale = Directory.EnumerateFiles(_backupDirectory)
                    .Select(path => new FileInfo(path))
                    .Where(info => SafeFileNameRegex.IsMatch(info.Name) &&
                                   info.Name.EndsWith(nameEnding, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(info => info.Name, StringComparer.Ordinal)
                    .Skip(Math.Max(keep, 1));

                foreach (var info in stale)
                    info.Delete();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Backup retention cleanup failed");
            }
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private string GetUploadsRoot()
        {
            var webRoot = !string.IsNullOrWhiteSpace(_webHost.WebRootPath)
                ? _webHost.WebRootPath
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(webRoot, UploadPaths.Root);
        }

        private string NewWorkDirectory()
        {
            var dir = Path.Combine(_backupDirectory, ".tmp-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch
            {
                // best effort; a leftover temp dir must not fail a finished backup/restore
            }
        }

        private static async Task<string> Sha256Async(string path, CancellationToken ct)
        {
            await using var stream = File.OpenRead(path);
            var hash = await SHA256.HashDataAsync(stream, ct);
            return Convert.ToHexString(hash);
        }

        private async Task RunToolAsync(
            string tool,
            ConnectionSettings connection,
            CancellationToken ct,
            IReadOnlyDictionary<string, string>? extraEnvironment,
            params string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = tool,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var argument in arguments)
                startInfo.ArgumentList.Add(argument);
            startInfo.Environment["PGPASSWORD"] = connection.Password;
            if (extraEnvironment is not null)
                foreach (var (key, value) in extraEnvironment)
                    startInfo.Environment[key] = value;

            using var process = new Process { StartInfo = startInfo };
            try
            {
                if (!process.Start())
                    throw new InvalidOperationException($"Failed to start {tool}.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"{tool} was not found. Install PostgreSQL client tools or configure its path under Backup:*Path.",
                    ex);
            }

            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            var stderr = await stderrTask;
            _ = await stdoutTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("{Tool} failed with exit code {ExitCode}: {Error}", tool, process.ExitCode, stderr);
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(stderr)
                        ? $"{tool} failed with exit code {process.ExitCode}."
                        : stderr.Trim());
            }
        }

        private static BackupFileDto ToDto(FileInfo info) => new()
        {
            FileName = info.Name,
            SizeBytes = info.Length,
            CreatedAtUtc = info.CreationTimeUtc,
            Kind = KindOf(info.Name),
        };

        private static string KindOf(string fileName)
        {
            if (fileName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) return "database";
            if (fileName.EndsWith(AutoSuffix + ".zip", StringComparison.OrdinalIgnoreCase)) return "auto";
            if (fileName.EndsWith(PreRestoreSuffix + ".zip", StringComparison.OrdinalIgnoreCase)) return "pre-restore";
            return "full";
        }

        private bool TryResolveSafePath(string fileName, out string filePath)
        {
            filePath = string.Empty;
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var name = Path.GetFileName(fileName);
            if (!SafeFileNameRegex.IsMatch(name))
                return false;

            filePath = Path.Combine(_backupDirectory, name);
            var fullPath = Path.GetFullPath(filePath);
            var fullDir = Path.GetFullPath(_backupDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return fullPath.StartsWith(fullDir, StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetDirectoryName(fullPath), Path.GetFullPath(_backupDirectory), StringComparison.OrdinalIgnoreCase);
        }

        private static ConnectionSettings ParseConnectionString(string connectionString)
        {
            var parts = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.Split('=', 2, StringSplitOptions.TrimEntries))
                .Where(pair => pair.Length == 2)
                .ToDictionary(pair => pair[0], pair => pair[1], StringComparer.OrdinalIgnoreCase);

            string GetRequired(params string[] keys)
            {
                foreach (var key in keys)
                {
                    if (parts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                        return value;
                }

                throw new InvalidOperationException(
                    $"Connection string is missing required key: {string.Join('/', keys)}");
            }

            var portText = parts.TryGetValue("Port", out var portValue) ? portValue : "5432";
            if (!int.TryParse(portText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
                port = 5432;

            return new ConnectionSettings(
                GetRequired("Host", "Server"),
                port,
                GetRequired("Database"),
                GetRequired("Username", "User Id", "UserID"),
                GetRequired("Password"));
        }

        private static string EscapeArg(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            if (value.Any(char.IsWhiteSpace) || value.Contains('"'))
                return $"\"{value.Replace("\"", "\\\"")}\"";

            return value;
        }

        private sealed record ConnectionSettings(
            string Host,
            int Port,
            string Database,
            string Username,
            string Password);

        private sealed class BackupManifest
        {
            public int Format { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public string Database { get; set; } = string.Empty;
            public string DumpSha256 { get; set; } = string.Empty;
            public int UploadsFileCount { get; set; }
            public string AppVersion { get; set; } = string.Empty;
        }
    }
}
