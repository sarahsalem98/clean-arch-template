using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Authorization;

public class PortalRequirement : IAuthorizationRequirement
{
    public string Portal { get; }

    public PortalRequirement(string portal) => Portal = portal;
}
