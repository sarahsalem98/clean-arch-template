using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Profile.Commands;

public record DeleteAccountCommand : IRequest<Result<object?>>
{
    public string Password { get; init; } = default!;
    public string? Reason { get; init; }
}

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public DeleteAccountCommandHandler(
        IApplicationDbContext context,
        IPasswordService passwordService,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<Result<object?>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.TokenInvalid();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            throw new DomainValidationException("password", "Password is incorrect.");

        // Soft delete
        user.IsDeleted = true;
        user.Status = UserStatus.Deleted;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletionScheduledAt = DateTime.UtcNow.AddDays(30);

        // Revoke all refresh tokens
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var rt in refreshTokens)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
        }

        // Remove all device tokens
        var deviceTokens = await _context.DeviceTokens
            .Where(dt => dt.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.DeviceTokens.RemoveRange(deviceTokens);

        _context.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            UserId = userId,
            Action = "ACCOUNT_DELETED",
            EntityName = "User",
            EntityId = userId.ToString(),
            NewValues = $"{{ \"reason\": \"{request.Reason}\" }}",
            IsSuccess = true
        });

        await _context.SaveChangesAsync(cancellationToken);

        _ = _emailService.SendAccountDeletionNoticeAsync(
            user.Email, user.FirstName, user.DeletionScheduledAt!.Value, CancellationToken.None);

        return Result<object?>.Success(null);
    }
}
