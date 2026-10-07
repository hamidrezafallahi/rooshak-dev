namespace Application.Common;

/// <summary>
/// Canonical on-disk / public media folders under wwwroot.
/// Convention: uploads/{entity-plural}/{entityId}/file.webp
/// Stored DB values are relative paths without a leading slash, e.g. uploads/brands/1/x.webp
/// Seed data may still use a leading slash or a flat file under uploads/{entity-plural}/name.ext
/// </summary>
public static class UploadPaths
{
    public const string Root = "uploads";

    public static string Brands(int id) => $"{Root}/brands/{id}";
    public static string Categories(int id) => $"{Root}/categories/{id}";
    public static string Products(int id) => $"{Root}/products/{id}";
    public static string Blogs(int id) => $"{Root}/blogs/{id}";
    public static string Users(int id) => $"{Root}/users/{id}";
    public static string LandingSlides(int id) => $"{Root}/landingslides/{id}";
    public static string AnnouncementBars(int id) => $"{Root}/announcementbars/{id}";

    /// <summary>
    /// Normalize any stored media path to: uploads/.../file.ext (no leading slash, forward slashes).
    /// </summary>
    public static string Normalize(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
            return string.Empty;

        return storedPath
            .Trim()
            .Replace('\\', '/')
            .TrimStart('/');
    }

    /// <summary>
    /// Split a stored media URL into (directory, fileName) for delete/upload operations.
    /// </summary>
    public static bool TryParse(string? storedPath, out string directory, out string fileName)
    {
        directory = string.Empty;
        fileName = string.Empty;

        var normalized = Normalize(storedPath);
        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        fileName = Path.GetFileName(normalized);
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var dir = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(dir))
            return false;

        directory = dir;
        return true;
    }

    /// <summary>
    /// True when UploadAs* returned a real relative path (not null / not an error message).
    /// </summary>
    public static bool IsStoredPath(string? uploadResult)
    {
        if (string.IsNullOrWhiteSpace(uploadResult))
            return false;

        var normalized = Normalize(uploadResult);
        return normalized.StartsWith($"{Root}/", StringComparison.OrdinalIgnoreCase);
    }
}
