using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Authorization;

public class PortalHandler : AuthorizationHandler<PortalRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PortalRequirement requirement)
    {
        var portal = context.User.FindFirst("portal")?.Value;

        if (string.Equals(portal, requirement.Portal, StringComparison.OrdinalIgnoreCase))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
