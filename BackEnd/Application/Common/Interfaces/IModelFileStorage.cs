using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces
{
    public sealed record StoredFileResult(string? Path, long SizeBytes, string? Error)
    {
        public bool Ok => Error == null && !string.IsNullOrWhiteSpace(Path);
    }

    /// <summary>
    /// Stores 3D model files (GLB / self-contained glTF / USDZ) and raw scan captures
    /// (photo / video) as-is. No transcoding: files are validated by extension + signature.
    /// </summary>
    public interface IModelFileStorage
    {
        /// <summary>glb or self-contained gltf for the web viewer / Android AR.</summary>
        Task<StoredFileResult> SaveModelAsync(IFormFile file, string relativeDirectory);

        /// <summary>usdz for iOS AR Quick Look.</summary>
        Task<StoredFileResult> SaveUsdzAsync(IFormFile file, string relativeDirectory);

        /// <summary>Phone capture used as photogrammetry input (jpg/png/webp/heic or mp4/mov/webm).</summary>
        Task<StoredFileResult> SaveScanSourceAsync(IFormFile file, string relativeDirectory);

        /// <summary>Kind of a stored scan file: "image" or "video".</summary>
        string ScanKind(string fileName);
    }
}
