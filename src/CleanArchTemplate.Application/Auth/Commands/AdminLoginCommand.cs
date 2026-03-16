using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record AdminLoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
}

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, Result<AuthResponseDto>>
{
    private static readonly HashSet<string> AllowedAdminRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "SuperAdmin"
    };

    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordService _passwordService;

    public AdminLoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtService,
        IPasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<Result<AuthResponseDto>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw UnauthorizedException.InvalidCredentials();

        if (user.Status == UserStatus.Deleted || user.IsDeleted)
            throw UnauthorizedException.AccountDeleted();

        if (user.Status == UserStatus.Suspended)
            throw UnauthorizedException.AccountSuspended();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var hasAdminRole = roles.Any(r => AllowedAdminRoles.Contains(r));

        if (!hasAdminRole)
            return Result<AuthResponseDto>.Forbidden("You do not have permission to access the admin portal.");

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = _jwtService.GenerateAccessToken(user, "admin-portal", roles, permissions);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "ADMIN_LOGIN",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            IsSuccess = true
        });

        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            User = new Auth.DTOs.UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
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
            }
        };

        return Result<AuthResponseDto>.Success(response);
    }
}
