using Microsoft.AspNetCore.Authorization;

namespace CleanArchTemplate.WebAPI.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}
