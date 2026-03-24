using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.WebAPI.Authorization;
using CleanArchTemplate.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace CleanArchTemplate.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers with global filters
        services.AddControllers(options =>
        {
            options.Filters.AddService<RateLimitHeaderFilter>();
        });
        services.AddEndpointsApiExplorer();

        // Authorization handlers
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        services.AddSingleton<IAuthorizationHandler, PortalHandler>();

        // Authorization policies
        services.AddAuthorization(options =>
        {
            // Permission policies — one per permission string
            var permissions = new[]
            {
                "profile.view", "profile.edit", "profile.image.upload", "account.delete",
                "users.view", "users.status.update", "users.role.assign", "audit-logs.view",
            };

            foreach (var permission in permissions)
            {
                options.AddPolicy($"Permission:{permission}", policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }

            // Portal policies
            options.AddPolicy("Portal:admin-portal", policy =>
                policy.Requirements.Add(new PortalRequirement("admin-portal")));

            options.AddPolicy("Portal:user-portal", policy =>
                policy.Requirements.Add(new PortalRequirement("user-portal")));
        });

        // Global action filter
        services.AddScoped<RateLimitHeaderFilter>();

        // Rate limiting
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfterSeconds = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    retryAfterSeconds = (int)Math.Ceiling(retryAfter.TotalSeconds);

                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(
                    ApiResponse.RateLimitFail(retryAfterSeconds), cancellationToken);
            };

            // Default sliding-window policy: 100 requests / 60 s
            options.AddSlidingWindowLimiter("default", opt =>
            {
                opt.PermitLimit = 100;
                opt.Window = TimeSpan.FromSeconds(60);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });

            // Strict policy for auth endpoints: 10 requests / 60 s
            options.AddSlidingWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromSeconds(60);
                opt.SegmentsPerWindow = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowConfiguredOrigins", policy =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? ["http://localhost:3000"];

                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Response compression (GZIP + Brotli)
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return services;
    }
}
