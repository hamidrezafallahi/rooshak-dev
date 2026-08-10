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
            if (request?.File is null || request.File.Length == 0)
                return null;

            var relativeDirectory = UploadPaths.Normalize(request.Path);
            if (string.IsNullOrWhiteSpace(relativeDirectory))
                return null;

            var sourceExt = Path.GetExtension(request.File.FileName).TrimStart('.');
            if (!string.IsNullOrWhiteSpace(sourceExt) && !SupportedTypes.Contains(sourceExt))
                return null;

            var rootPath = ResolveDirectory(relativeDirectory);
            Directory.CreateDirectory(rootPath);

            var safeBaseName = Path.GetFileNameWithoutExtension(request.File.FileName);
            if (string.IsNullOrWhiteSpace(safeBaseName))
                safeBaseName = "image";

            var fileName = $"{Guid.NewGuid():N}_{safeBaseName}{extension}";
            var systemFilePath = Path.Combine(rootPath, fileName);

            await using (var input = request.File.OpenReadStream())
            using (var image = await Image.LoadAsync(input))
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
