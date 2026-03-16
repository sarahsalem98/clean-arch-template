using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record SocialLoginCommand : IRequest<Result<SocialAuthResponseDto>>
{
    public string Provider { get; init; } = default!;
    public string AccessToken { get; init; } = default!;
    public string? DeviceType { get; init; }
    public string? DeviceToken { get; init; }
}

public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, Result<SocialAuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;

    public SocialLoginCommandHandler(IApplicationDbContext context, IJwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<SocialAuthResponseDto>> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        // Stub: verify token with provider
        var (providerUserId, providerEmail, providerName) = await VerifyProviderTokenAsync(
            request.Provider, request.AccessToken, cancellationToken);

        if (!Enum.TryParse<SocialProvider>(request.Provider, true, out var provider))
            throw new Domain.Exceptions.DomainValidationException("provider",
                "Provider must be 'google', 'facebook', or 'apple'.");

        // Check for existing social login
        var existingSocialLogin = await _context.SocialLogins
            .Include(sl => sl.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(sl =>
                sl.Provider == provider && sl.ProviderUserId == providerUserId, cancellationToken);

        User user;
        var isNewUser = false;

        if (existingSocialLogin != null)
        {
            user = existingSocialLogin.User;
        }
        else
        {
            // Check by email
            user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == providerEmail.ToLowerInvariant(), cancellationToken)!;

            if (user == null)
            {
                isNewUser = true;
                var nameParts = providerName.Split(' ', 2);
                user = new User
                {
                    Email = providerEmail.ToLowerInvariant(),
                    PasswordHash = string.Empty,
                    FirstName = nameParts[0],
                    LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                    IsVerified = true,
                    Status = UserStatus.Active
                };
                _context.Users.Add(user);

                var userRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
                if (userRole != null)
                    _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
            }

            _context.SocialLogins.Add(new SocialLogin
            {
                UserId = user.Id,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail,
                AccessToken = request.AccessToken
            });
        }

        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            var exists = await _context.DeviceTokens
                .AnyAsync(dt => dt.UserId == user.Id && dt.Token == request.DeviceToken, cancellationToken);
            if (!exists)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId = user.Id,
                    Token = request.DeviceToken,
                    DeviceType = request.DeviceType ?? "unknown",
                    LastUsedAt = DateTime.UtcNow
                });
            }
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        if (!roles.Any()) roles = new List<string> { "User" };
        if (!permissions.Any()) permissions = new List<string> { "profile.view", "profile.edit", "profile.image.upload", "account.delete" };

        var accessToken = _jwtService.GenerateAccessToken(user, "user-portal", roles, permissions);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<SocialAuthResponseDto>.Success(new SocialAuthResponseDto
        {
            User = new Auth.DTOs.UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImage = user.ProfileImage,
                IsVerified = user.IsVerified,
                Status = user.Status.ToString().ToLower(),
                CreatedAt = user.CreatedAt
            },
            Tokens = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenStr,
                ExpiresIn = 3600,
                TokenType = "Bearer"
            },
            IsNewUser = isNewUser,
            Provider = request.Provider.ToLower()
        });
    }

    private static Task<(string UserId, string Email, string Name)> VerifyProviderTokenAsync(
        string provider, string accessToken, CancellationToken cancellationToken)
    {
        // Stub: In production, call provider's token verification API
        return Task.FromResult(($"provider_user_{Guid.NewGuid():N}", "social@example.com", "Social User"));
    }
}
