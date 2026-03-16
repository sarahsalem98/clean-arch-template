using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record RefreshTokenCommand : IRequest<Result<RefreshTokenResponseDto>>
{
    public string RefreshToken { get; init; } = default!;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken == null || existingToken.IsRevoked)
            throw UnauthorizedException.TokenInvalid();

        if (existingToken.IsExpired)
            throw UnauthorizedException.TokenExpired();

        var user = existingToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

        // Determine portal from the existing token's user context (check existing active token if needed)
        // Default to user-portal; admin portal is separate
        var portal = "user-portal";

        var newAccessToken = _jwtService.GenerateAccessToken(user, portal, roles, permissions);
        var newRefreshTokenStr = _jwtService.GenerateRefreshToken();

        // Rotate: revoke old, issue new
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshTokenStr;

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponseDto>.Success(new RefreshTokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenStr,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}
