using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;

namespace CleanArchTemplate.WebAPI.Filters;

public class RateLimitHeaderFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        var rateLimitAttr = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();
        if (rateLimitAttr is not null)
            context.HttpContext.Response.Headers["X-RateLimit-Policy"] = rateLimitAttr.PolicyName;
    }
}
