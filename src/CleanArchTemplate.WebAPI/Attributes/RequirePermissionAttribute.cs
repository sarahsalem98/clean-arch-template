using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Attributes;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base($"Permission:{permission}")
    {
    }
}
