using System.Buffers.Binary;
using System.Text.Json;
using Application.Common;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Services.Services.Uploader
{
    public class ModelFileStorage : IModelFileStorage
    {
        public const long MaxModelBytes = 25L * 1024 * 1024;
        public const long MaxScanBytes = 50L * 1024 * 1024;

        private static readonly HashSet<string> ModelTypes = new(StringComparer.OrdinalIgnoreCase) { "glb", "gltf" };
        private static readonly HashSet<string> ScanImageTypes = new(StringComparer.OrdinalIgnoreCase) { "jpg", "jpeg", "png", "webp", "heic", "heif" };
        private static readonly HashSet<string> ScanVideoTypes = new(StringComparer.OrdinalIgnoreCase) { "mp4", "mov", "webm" };

        private readonly IWebHostEnvironment _webHost;

        public ModelFileStorage(IWebHostEnvironment webHost) => _webHost = webHost;

        public async Task<StoredFileResult> SaveModelAsync(IFormFile file, string relativeDirectory)
        {
            var ext = ExtensionOf(file);
            if (!ModelTypes.Contains(ext))
                return Fail("فرمت مدل باید GLB (یا glTF تک‌فایلی) باشد.");
            if (file.Length == 0 || file.Length > MaxModelBytes)
                return Fail($"حجم مدل باید کمتر از {MaxModelBytes / 1024 / 1024} مگابایت باشد.");

            var bytes = await ReadAllAsync(file);
            var error = ext == "glb" ? ValidateGlb(bytes) : ValidateGltf(bytes);
            if (error != null)
                return Fail(error);

            return await WriteAsync(new MemoryStream(bytes, writable: false), relativeDirectory, ext);
        }

        public async Task<StoredFileResult> SaveUsdzAsync(IFormFile file, string relativeDirectory)
        {
            if (ExtensionOf(file) != "usdz")
                return Fail("فرمت فایل iOS باید USDZ باشد.");
            if (file.Length == 0 || file.Length > MaxModelBytes)
                return Fail($"حجم فایل USDZ باید کمتر از {MaxModelBytes / 1024 / 1024} مگابایت باشد.");

            var bytes = await ReadAllAsync(file);
            // USDZ is an uncompressed zip archive ("PK\x03\x04").
            if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B || bytes[2] != 0x03 || bytes[3] != 0x04)
                return Fail("فایل USDZ معتبر نیست.");

            return await WriteAsync(new MemoryStream(bytes, writable: false), relativeDirectory, "usdz");
        }

        public async Task<StoredFileResult> SaveScanSourceAsync(IFormFile file, string relativeDirectory)
        {
            var ext = ExtensionOf(file);
            if (!ScanImageTypes.Contains(ext) && !ScanVideoTypes.Contains(ext))
                return Fail("فقط عکس (jpg, png, webp, heic) یا ویدیو (mp4, mov, webm) پذیرفته می‌شود.");
            if (file.Length == 0 || file.Length > MaxScanBytes)
                return Fail($"حجم هر فایل باید کمتر از {MaxScanBytes / 1024 / 1024} مگابایت باشد.");

            await using var input = file.OpenReadStream();
            return await WriteAsync(input, relativeDirectory, ext);
        }

        public string ScanKind(string fileName)
        {
            var ext = Path.GetExtension(fileName).TrimStart('.');
            return ScanVideoTypes.Contains(ext) ? "video" : "image";
        }

        // ── validation ────────────────────────────────────────────────────────

        private static string? ValidateGlb(byte[] b)
        {
            // Header: magic "glTF", uint32 version (2), uint32 total length.
            if (b.Length < 20 || b[0] != 0x67 || b[1] != 0x6C || b[2] != 0x54 || b[3] != 0x46)
                return "فایل GLB معتبر نیست.";
            if (BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(4, 4)) != 2)
                return "فقط glTF نسخه ۲ پشتیبانی می‌شود.";
            if (BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(8, 4)) > b.Length)
                return "فایل GLB ناقص است.";
            return null;
        }

        private static string? ValidateGltf(byte[] b)
        {
            try
            {
                using var doc = JsonDocument.Parse(b);
                var root = doc.RootElement;
                if (!root.TryGetProperty("asset", out var asset) ||
                    !asset.TryGetProperty("version", out var ver) ||
                    ver.GetString()?.StartsWith('2') != true)
                    return "فقط glTF نسخه ۲ پشتیبانی می‌شود.";

                foreach (var section in new[] { "buffers", "images" })
                {
                    if (!root.TryGetProperty(section, out var items)) continue;
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("uri", out var uri) &&
                            uri.GetString()?.StartsWith("data:", StringComparison.OrdinalIgnoreCase) != true)
                            return "فایل glTF به فایل‌های جانبی وابسته است؛ لطفاً آن را به صورت GLB (تک‌فایل) صادر کنید.";
                    }
                }
                return null;
            }
            catch (JsonException)
            {
                return "فایل glTF معتبر نیست.";
            }
        }

        // ── io ────────────────────────────────────────────────────────────────

        private static string ExtensionOf(IFormFile file) =>
            Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();

        private static async Task<byte[]> ReadAllAsync(IFormFile file)
        {
            await using var input = file.OpenReadStream();
            using var ms = new MemoryStream((int)file.Length);
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }

        private async Task<StoredFileResult> WriteAsync(Stream content, string relativeDirectory, string ext)
        {
            await using var _ = content;
            var dir = UploadPaths.Normalize(relativeDirectory);
            if (string.IsNullOrWhiteSpace(dir))
                return Fail("مسیر ذخیره‌سازی نامعتبر است.");

            var segments = dir.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var absolute = Path.Combine(new[] { ResolveWwwRoot() }.Concat(segments).ToArray());
            Directory.CreateDirectory(absolute);

            var fileName = $"{Guid.NewGuid():N}.{ext}";
            long size;
            await using (var output = new FileStream(Path.Combine(absolute, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await content.CopyToAsync(output);
                size = output.Length;
            }
            return new StoredFileResult($"{dir}/{fileName}", size, null);
        }

        private string ResolveWwwRoot() =>
            !string.IsNullOrWhiteSpace(_webHost.WebRootPath)
                ? _webHost.WebRootPath
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        private static StoredFileResult Fail(string error) => new(null, 0, error);
    }
}
