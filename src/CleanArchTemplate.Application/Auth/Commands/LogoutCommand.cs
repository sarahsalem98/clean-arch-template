using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record LogoutCommand : IRequest<Result<object?>>
{
    public string? DeviceToken { get; init; }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<object?>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.TokenInvalid();

        // Revoke all refresh tokens
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var rt in refreshTokens)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
        }

        // Remove specific device token
        if (!string.IsNullOrWhiteSpace(request.DeviceToken))
        {
            var deviceToken = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == request.DeviceToken, cancellationToken);

            if (deviceToken != null)
                _context.DeviceTokens.Remove(deviceToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
