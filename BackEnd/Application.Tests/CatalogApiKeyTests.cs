using System.Security.Claims;
using Application.Common;
using Xunit;

namespace Application.Tests;

public class CatalogApiKeyTests
{
    [Fact]
    public void Matches_equal_secrets()
    {
        Assert.True(CatalogApiKey.Matches("catalog-key-one", "catalog-key-one"));
    }

    [Fact]
    public void Rejects_mismatch_empty_and_different_length()
    {
        Assert.False(CatalogApiKey.Matches("catalog-key-one", "catalog-key-two"));
        Assert.False(CatalogApiKey.Matches(null, "catalog-key-one"));
        Assert.False(CatalogApiKey.Matches("catalog-key-one", null));
        Assert.False(CatalogApiKey.Matches("", "catalog-key-one"));
        Assert.False(CatalogApiKey.Matches("catalog-key-one", ""));
        Assert.False(CatalogApiKey.Matches("short", "much-longer-secret"));
    }

    [Fact]
    public void Principal_is_content_editor_with_service_user_id()
    {
        var principal = CatalogApiKey.CreatePrincipal(42, "content-bot@onlineshop.local", "Content Admin");

        Assert.Equal("42", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("ContentEditor", principal.FindFirst(ClaimTypes.Role)?.Value);
        Assert.True(principal.IsInRole("ContentEditor"));
        Assert.Equal(CatalogApiKey.SchemeName, principal.Identity?.AuthenticationType);
    }
}
