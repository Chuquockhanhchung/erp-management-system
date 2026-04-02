using ERP.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERP.API.Security;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context, IAuditService audit)
    {
        await _next(context);

        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            var userIdStr = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
             ?? context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            int? userId = int.TryParse(userIdStr, out var id) ? id : null;

            await audit.WriteAuditLogAsync(
                "API",
                $"{context.Request.Method} {context.Request.Path}",
                null,
                $"StatusCode={context.Response.StatusCode}",
                userId
            );
        }
    }
}