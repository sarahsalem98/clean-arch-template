using CleanArchTemplate.Application.Common.Models;
using System.Text.Json;

namespace CleanArchTemplate.WebAPI.Middleware;

public class PortalValidationMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Maps URL path prefix → expected portal claim value
    private static readonly Dictionary<string, string> PortalMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/api/v1/admin"] = "admin-portal",
        ["/api/v2/admin"] = "admin-portal",
        ["/api/v1/user"]  = "user-portal",
        ["/api/v2/user"]  = "user-portal",
    };

    public PortalValidationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        var matchedPortal = PortalMap
            .FirstOrDefault(kv => path.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
            .Value;

        if (matchedPortal != null && context.User.Identity?.IsAuthenticated == true)
        {
            var portalClaim = context.User.FindFirst("portal")?.Value;
            if (!string.Equals(portalClaim, matchedPortal, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.Fail(
                    ErrorCodes.ResourceAccessDenied,
                    $"This token is not valid for the {matchedPortal}.");

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response, JsonOptions));
                return;
            }
        }

        await _next(context);
    }
}
