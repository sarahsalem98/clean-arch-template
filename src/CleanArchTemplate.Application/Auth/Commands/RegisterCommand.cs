using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record RegisterCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string? Phone { get; init; }
    public string? DeviceType { get; init; }
    public string? DeviceToken { get; init; }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtService,
        IPasswordService passwordService,
        IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _emailService = emailService;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users
            .AnyAsync(u => u.Email == emailNormalized && !u.IsDeleted, cancellationToken);

        if (exists)
            return Result<AuthResponseDto>.Conflict("An account with this email already exists.");

        var user = new User
        {
            Email = emailNormalized,
            PasswordHash = _passwordService.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = request.Phone?.Trim(),
            IsVerified = false,
            Status = UserStatus.Active,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        _context.Users.Add(user);

        // Assign default User role
        var userRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);

        if (userRole != null)
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });

        // Save device token
        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            _context.DeviceTokens.Add(new DeviceToken
            {
                UserId = user.Id,
                Token = request.DeviceToken,
                DeviceType = request.DeviceType ?? "unknown",
                LastUsedAt = DateTime.UtcNow
            });
        }

        // Load permissions for User role
        var permissions = new List<string> { "profile.view", "profile.edit", "profile.image.upload", "account.delete" };
        var roles = new List<string> { "User" };

        var accessToken = _jwtService.GenerateAccessToken(user, "user-portal", roles, permissions);
        var refreshTokenStr = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Send verification email (stub – fire and forget)
        _ = _emailService.SendVerificationEmailAsync(
            user.Email, user.FirstName, user.EmailVerificationToken!, CancellationToken.None);

        var response = new AuthResponseDto
        {
            User = MapUser(user),
            Tokens = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenStr,
                ExpiresIn = 3600,
                TokenType = "Bearer"
            }
        };

        return Result<AuthResponseDto>.Success(response, 201);
    }

    private static Auth.DTOs.UserDto MapUser(User user) => new()
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
    };
}

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Phone must be in E.164 format (e.g. +12125552368).")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.DeviceType)
            .Must(dt => dt == null || dt == "ios" || dt == "android")
            .WithMessage("DeviceType must be 'ios' or 'android'.");
    }
}
