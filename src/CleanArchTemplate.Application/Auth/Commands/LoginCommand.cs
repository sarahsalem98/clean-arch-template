using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? DeviceType { get; init; }
    public string? DeviceToken { get; init; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordService _passwordService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtService,
        IPasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Always perform password check to avoid timing attacks
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw UnauthorizedException.InvalidCredentials();

        if (user.Status == UserStatus.Deleted || user.IsDeleted)
            throw UnauthorizedException.AccountDeleted();

        if (user.Status == UserStatus.Suspended)
            throw UnauthorizedException.AccountSuspended();

        if (!user.IsVerified)
            throw UnauthorizedException.AccountNotVerified();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = _jwtService.GenerateAccessToken(user, "user-portal", roles, permissions);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        // Save/update device token
        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            var existingDevice = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == user.Id && dt.Token == request.DeviceToken, cancellationToken);

            if (existingDevice == null)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId = user.Id,
                    Token = request.DeviceToken,
                    DeviceType = request.DeviceType ?? "unknown",
                    LastUsedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingDevice.LastUsedAt = DateTime.UtcNow;
            }
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "LOGIN",
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
                ThumbnailImage = user.ThumbnailImage,
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

public record LoginV2Command : IRequest<Result<AuthResponseV2Dto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? DeviceType { get; init; }
    public string? DeviceToken { get; init; }
}

public class LoginV2CommandHandler : IRequestHandler<LoginV2Command, Result<AuthResponseV2Dto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordService _passwordService;

    public LoginV2CommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtService,
        IPasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    public async Task<Result<AuthResponseV2Dto>> Handle(LoginV2Command request, CancellationToken cancellationToken)
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

        if (!user.IsVerified)
            throw UnauthorizedException.AccountNotVerified();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        var accessToken = _jwtService.GenerateAccessToken(user, "user-portal", roles, permissions);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            var existingDevice = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == user.Id && dt.Token == request.DeviceToken, cancellationToken);

            if (existingDevice == null)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    UserId = user.Id,
                    Token = request.DeviceToken,
                    DeviceType = request.DeviceType ?? "unknown",
                    LastUsedAt = DateTime.UtcNow
                });
            }
            else
            {
                existingDevice.LastUsedAt = DateTime.UtcNow;
            }
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "LOGIN",
            EntityName = "User",
            EntityId = user.Id.ToString(),
            IsSuccess = true
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseV2Dto>.Success(new AuthResponseV2Dto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                ThumbnailImage = user.ThumbnailImage,
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
            Roles = roles,
            Permissions = permissions
        });
    }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
