using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Common;

/// <summary>
/// Shared secret for n8n catalog writes. Not a user password and not a login JWT.
/// </summary>
public static class CatalogApiKey
{
    public const string HeaderName = "X-Api-Key";
    public const string SchemeName = "ApiKey";
    public const string PolicyName = "CatalogWrite";
    public const string RoleName = "ContentEditor";

    public static bool Matches(string? configured, string? provided)
    {
        if (string.IsNullOrEmpty(configured) || string.IsNullOrEmpty(provided))
            return false;

        var left = Encoding.UTF8.GetBytes(configured);
        var right = Encoding.UTF8.GetBytes(provided);
        if (left.Length != right.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(left, right);
    }

    public static ClaimsPrincipal CreatePrincipal(int userId, string email, string fullName)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, fullName ?? string.Empty),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim(ClaimTypes.Role, RoleName)
            ],
            SchemeName);

        return new ClaimsPrincipal(identity);
    }
}
