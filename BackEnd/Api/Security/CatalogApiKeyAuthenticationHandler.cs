using System.Text.Encodings.Web;
using Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OnlineShop.Domain.Interfaces;

namespace Api.Security;

/// <summary>
/// Authenticates n8n catalog writes via X-Api-Key. Missing header is NoResult so JWT still works.
/// </summary>
public sealed class CatalogApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserRepository users,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(CatalogApiKey.HeaderName, out var headerValues))
            return AuthenticateResult.NoResult();

        var provided = headerValues.ToString();
        var configured = configuration["ContentAutomation:ApiKey"];
        if (!CatalogApiKey.Matches(configured, provided))
            return AuthenticateResult.Fail("Invalid API key");

        var email = configuration["ContentAutomation:ServiceAccountEmail"] ?? "content-bot@onlineshop.local";
        var user = await users.GetByEmailAsync(email);
        if (user is null || user.IsDeleted || !user.IsActive)
            return AuthenticateResult.Fail("Catalog service account is missing or inactive");

        var principal = CatalogApiKey.CreatePrincipal(user.Id, user.Email, user.FullName);
        var ticket = new AuthenticationTicket(principal, CatalogApiKey.SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}
