using CleanArchTemplate.Application.Common.Models;
using System.Text.Json;

namespace CleanArchTemplate.WebAPI.Middleware;

public class HttpsEnforcementMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HttpsEnforcementMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.IsHttps)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.Fail(
                "HTTPS_REQUIRED",
                "HTTPS is required. Plain HTTP requests are not accepted.");

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, JsonOptions));
            return;
        }

        await _next(context);
    }
}
