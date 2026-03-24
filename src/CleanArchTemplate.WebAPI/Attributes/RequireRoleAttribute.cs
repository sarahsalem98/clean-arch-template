using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Attributes;

public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}
