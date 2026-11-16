using OnlineShop.Domain.Common;
using Application.Common;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Services.Services.Uploader.DTO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace Services.Services.Uploader
{
    public class UploaderService : IUploaderService
    {
        private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "webp"
        };

        private static readonly HashSet<string> VideoTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "mp4", "webm"
        };

        /// <summary>Hero videos should be short loops; keep uploads small for mobile data.</summary>
        private const long MaxVideoBytes = 30L * 1024 * 1024;

        private readonly IWebHostEnvironment _webHost;

        public UploaderService(IWebHostEnvironment webHostEnvironment)
        {
            _webHost = webHostEnvironment;
        }

        public Task<string?> UploadAsJpg(UploadDTO request) =>
            UploadAsync(request, ".jpg", async (image, stream) =>
                await image.SaveAsync(stream, new JpegEncoder { Quality = 80 }));

        public Task<string?> UploadAsJpeg(UploadDTO request) =>
            UploadAsync(request, ".jpeg", async (image, stream) =>
                await image.SaveAsync(stream, new JpegEncoder { Quality = 80 }));

        public Task<string?> UploadAsPng(UploadDTO request) =>
            UploadAsync(request, ".png", async (image, stream) =>
                await image.SaveAsync(stream, new PngEncoder()));

        public Task<string?> UploadAsWebp(UploadDTO request) =>
            UploadAsync(request, ".webp", async (image, stream) =>
                await image.SaveAsync(stream, new WebpEncoder
                {
                    Quality = 70,
                    FileFormat = WebpFileFormatType.Lossy,
                }));

        public async Task<string?> UploadVideo(UploadDTO request)

        {

            if (request?.File is null)

                return null;

            EnsureNotEmpty(request.File.FileName, request.File.Length);

            if (request.File.Length > MaxVideoBytes)

                throw new FileUploadException(
                    $"حجم ویدیوی «{request.File.FileName}» بیشتر از سقف مجاز ۳۰ مگابایت است.");


            var relativeDirectory = UploadPaths.Normalize(request.Path);

            if (string.IsNullOrWhiteSpace(relativeDirectory))

                throw new FileUploadException("مسیر ذخیرهٔ فایل مشخص نشده است.");


            var ext = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();

            if (!VideoTypes.Contains(ext))

                throw new FileUploadException(
                    $"فرمت فایل «{request.File.FileName}» پشتیبانی نمی‌شود؛ برای ویدیو فقط mp4 و webm مجاز است.");


            var rootPath = ResolveDirectory(relativeDirectory);

            Directory.CreateDirectory(rootPath);


            var fileName = $"{Guid.NewGuid():N}_hero.{ext}";

            await using (var input = request.File.OpenReadStream())

            await using (var output = new FileStream(

                Path.Combine(rootPath, fileName),

                FileMode.Create,

                FileAccess.Write,

                FileShare.None))

            {

                await input.CopyToAsync(output);

            }


            return $"{relativeDirectory}/{fileName}";

        }

        /// <summary>Font files are served as-is from /uploads; keep them small for page speed.</summary>
        private const long MaxFontBytes = 5L * 1024 * 1024;

        public async Task<string?> UploadFont(UploadDTO request)
        {
            if (request?.File is null)
                return null;

            EnsureNotEmpty(request.File.FileName, request.File.Length);
            if (request.File.Length > MaxFontBytes)
                throw new FileUploadException($"حجم فونت «{request.File.FileName}» بیشتر از سقف مجاز ۵ مگابایت است.");

            var relativeDirectory = UploadPaths.Normalize(request.Path);
            if (string.IsNullOrWhiteSpace(relativeDirectory))
                throw new FileUploadException("مسیر ذخیرهٔ فایل مشخص نشده است.");

            var ext = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
            var header = new byte[4];
            await using (var probe = request.File.OpenReadStream())
            {
                var read = await probe.ReadAsync(header.AsMemory(0, 4));
                if (read < 4 || !FontSignatureMatches(ext, header))
                    throw new FileUploadException(
                        $"فایل «{request.File.FileName}» فونت معتبر نیست؛ فقط woff2، woff، ttf و otf پذیرفته می‌شود.");
            }

            var rootPath = ResolveDirectory(relativeDirectory);
            Directory.CreateDirectory(rootPath);

            var fileName = $"{Guid.NewGuid():N}.{ext}";
            await using (var input = request.File.OpenReadStream())
            await using (var output = new FileStream(Path.Combine(rootPath, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await input.CopyToAsync(output);
            }

            return $"{relativeDirectory}/{fileName}";
        }

        /// <summary>The extension must agree with the file's magic bytes, so nothing else is served as a font.</summary>
        private static bool FontSignatureMatches(string ext, byte[] h) => ext switch
        {
            "woff2" => h[0] == (byte)'w' && h[1] == (byte)'O' && h[2] == (byte)'F' && h[3] == (byte)'2',
            "woff" => h[0] == (byte)'w' && h[1] == (byte)'O' && h[2] == (byte)'F' && h[3] == (byte)'F',
            "otf" => h[0] == (byte)'O' && h[1] == (byte)'T' && h[2] == (byte)'T' && h[3] == (byte)'O',
            "ttf" => (h[0] == 0x00 && h[1] == 0x01 && h[2] == 0x00 && h[3] == 0x00)
                     || (h[0] == (byte)'t' && h[1] == (byte)'r' && h[2] == (byte)'u' && h[3] == (byte)'e'),
            _ => false,
        };


        public Task DeleteFile(DeleteDTO request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.Path) ||
                string.IsNullOrWhiteSpace(request.FileName))
            {
                return Task.CompletedTask;
            }

            var rootPath = ResolveDirectory(request.Path);
            if (!Directory.Exists(rootPath))
                return Task.CompletedTask;

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(request.FileName);
            if (string.IsNullOrWhiteSpace(fileNameWithoutExt))
                return Task.CompletedTask;

            var targetFile = Directory.GetFiles(rootPath)
                .FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f)
                        .Equals(fileNameWithoutExt, StringComparison.OrdinalIgnoreCase));

            if (targetFile is not null)
            {
                try { File.Delete(targetFile); }
                catch { /* best-effort cleanup */ }
            }

            return Task.CompletedTask;
        }

        public async Task DeleteStoredFile(string? storedPath, string fallbackDirectory)
        {
            string directory;
            string fileName;

            if (UploadPaths.TryParse(storedPath, out var parsedDirectory, out var parsedFileName))
            {
                directory = parsedDirectory;
                fileName = parsedFileName;
            }
            else
            {
                directory = UploadPaths.Normalize(fallbackDirectory);
                fileName = Path.GetFileName(UploadPaths.Normalize(storedPath));
                if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
                    return;
            }

            await DeleteFile(new DeleteDTO
            {
                Path = directory,
                FileName = fileName
            });
        }

        private async Task<string?> UploadAsync(
            UploadDTO request,
            string extension,
            Func<Image, Stream, Task> saveAsync)
        {
            if (request?.File is null)
                return null;

            EnsureNotEmpty(request.File.FileName, request.File.Length);

            var relativeDirectory = UploadPaths.Normalize(request.Path);
            if (string.IsNullOrWhiteSpace(relativeDirectory))
                throw new FileUploadException("مسیر ذخیرهٔ فایل مشخص نشده است.");

            var sourceExt = Path.GetExtension(request.File.FileName).TrimStart('.');
            if (!string.IsNullOrWhiteSpace(sourceExt) && !SupportedTypes.Contains(sourceExt))
                throw new FileUploadException(
                    $"فرمت فایل «{request.File.FileName}» پشتیبانی نمی‌شود؛ برای تصویر فقط jpg، jpeg، png و webp مجاز است.");

            var rootPath = ResolveDirectory(relativeDirectory);
            Directory.CreateDirectory(rootPath);

            var safeBaseName = Path.GetFileNameWithoutExtension(request.File.FileName);
            if (string.IsNullOrWhiteSpace(safeBaseName))
                safeBaseName = "image";

            var fileName = $"{Guid.NewGuid():N}_{safeBaseName}{extension}";
            var systemFilePath = Path.Combine(rootPath, fileName);

            await using var input = request.File.OpenReadStream();
            using var image = await LoadImageAsync(input, request.File.FileName);
            await using (var output = new FileStream(
                systemFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                await saveAsync(image, output);
            }

            // Canonical DB/public path: uploads/{entity}/{id}/file.webp (no leading slash)
            return $"{relativeDirectory}/{fileName}";
        }

        private static async Task<Image> LoadImageAsync(Stream input, string fileName)
        {
            try
            {
                return await Image.LoadAsync(input);
            }
            catch (Exception ex) when (ex is ImageFormatException or NotSupportedException)
            {
                throw new FileUploadException(
                    $"فایل «{fileName}» تصویر معتبری نیست یا خراب است؛ یک فایل jpg، png یا webp سالم انتخاب کنید.",
                    ex);
            }
        }

        private static void EnsureNotEmpty(string fileName, long length)
        {
            if (length == 0)
                throw new FileUploadException($"فایل «{fileName}» خالی است.");
        }

        private string ResolveWwwRoot()
        {
            if (!string.IsNullOrWhiteSpace(_webHost.WebRootPath))
                return _webHost.WebRootPath;

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        private string ResolveDirectory(string relativeDirectory)
        {
            var normalized = UploadPaths.Normalize(relativeDirectory);
            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return Path.Combine(new[] { ResolveWwwRoot() }.Concat(segments).ToArray());
        }
    }
}
