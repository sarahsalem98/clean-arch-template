using System.Security.Claims;
using CleanArchTemplate.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CleanArchTemplate.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User?.FindFirstValue("sub");
            return sub != null && Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                         ?? User?.FindFirstValue("email");

    public string? Portal => User?.FindFirstValue("portal");

    public string? JwtId => User?.FindFirstValue("jti");

    public IList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        ?? [];

    public IList<string> Permissions =>
        User?.FindAll("permission").Select(c => c.Value).ToList()
        ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
