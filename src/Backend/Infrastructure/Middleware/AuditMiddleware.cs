using System.Security.Claims;
using Backend.Application.Interfaces;

namespace Backend.Infrastructure.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;

    public AuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Skip logging for static files or swagger if desired, but for now log everything
        // Maybe skip /hangfire to avoid noise if dashboard is open
        if (context.Request.Path.StartsWithSegments("/hangfire") || 
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        await _next(context);

        var path = context.Request.Path;
        var method = context.Request.Method;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
        Guid? userId = null;
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedId))
        {
            userId = parsedId;
        }

        await auditService.LogAsync(
            action: "HttpRequest",
            resource: $"{method} {path}",
            details: $"Status: {context.Response.StatusCode}",
            userId: userId,
            ipAddress: ipAddress,
            userAgent: userAgent
        );
    }
}
