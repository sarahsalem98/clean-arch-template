using System.Security.Claims;
using CleanArchTemplate.Domain.Entities;

namespace CleanArchTemplate.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string portal, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string? GetClaimValue(string token, string claimType);
    bool IsTokenValid(string token);
}
