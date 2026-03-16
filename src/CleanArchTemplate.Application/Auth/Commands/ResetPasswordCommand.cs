using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record ResetPasswordCommand : IRequest<Result<object?>>
{
    public string Token { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
    public string ConfirmPassword { get; init; } = default!;
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public ResetPasswordCommandHandler(IApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<Result<object?>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new DomainValidationException("confirmPassword", "Passwords do not match.");

        // Find user with this reset token
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.PasswordResetToken == request.Token &&
                u.PasswordResetTokenExpiry > DateTime.UtcNow &&
                !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            // Also check PasswordResetTokens table
            var tokenRecord = await _context.PasswordResetTokens
                .Include(t => t.User)
                .Where(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            var matchedRecord = tokenRecord.FirstOrDefault(t =>
                _passwordService.VerifyPassword(request.Token, t.TokenHash));

            if (matchedRecord == null)
                throw new DomainValidationException("token", "Token is invalid or has expired.");

            user = matchedRecord.User;
            matchedRecord.IsUsed = true;
            matchedRecord.UsedAt = DateTime.UtcNow;
        }

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        // Revoke all refresh tokens for security
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var rt in refreshTokens)
        {
            rt.IsRevoked = true;
            rt.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.");
    }
}
