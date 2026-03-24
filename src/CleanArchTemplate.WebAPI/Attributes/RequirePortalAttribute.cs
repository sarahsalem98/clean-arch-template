using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Attributes;

public class RequirePortalAttribute : AuthorizeAttribute
{
    public RequirePortalAttribute(string portal)
        : base($"Portal:{portal}")
    {
    }
}
