using Api.Errors;
using Common;
using OnlineShop.Infrastructure.Security;

public class BlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public BlacklistMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        using var scope = _scopeFactory.CreateScope();
        var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();

        var isBlacklisted = await securityService.IsIpBlacklistedAsync(ip);
        if (isBlacklisted)
        {
            await ApiErrorWriter.WriteAsync(
                context,
                StatusCodes.Status403Forbidden,
                ErrorCodes.IpBlocked,
                "دسترسی آی‌پی شما به این سرویس مسدود شده است.");
            return;
        }

        await _next(context);
    }
}
