using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArchTemplate.WebAPI.Filters;

public class RateLimitHeaderFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var httpContext = context.HttpContext;

        // Expose rate-limit metadata set by ASP.NET Core's RateLimiter
        if (httpContext.Features.Get<IRateLimiterStatisticsFeature>() is { } stats)
        {
            httpContext.Response.Headers["X-RateLimit-Limit"] =
                (stats.CurrentAvailablePermits + (stats.TotalSuccessfulLeasesCount > 0 ? 1 : 0)).ToString();
            httpContext.Response.Headers["X-RateLimit-Remaining"] =
                stats.CurrentAvailablePermits.ToString();
        }
    }
}
