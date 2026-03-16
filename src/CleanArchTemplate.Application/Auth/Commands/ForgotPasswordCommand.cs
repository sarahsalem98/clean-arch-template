using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record ForgotPasswordCommand : IRequest<Result<ForgotPasswordResponseDto>>
{
    public string Email { get; init; } = default!;
}

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = default!;
    public int ResetTokenExpiresIn { get; set; } = 900;
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordService _passwordService;

    public ForgotPasswordCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IPasswordService passwordService)
    {
        _context = context;
        _emailService = emailService;
        _passwordService = passwordService;
    }

    public async Task<Result<ForgotPasswordResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Always return success (prevent email enumeration)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

        if (user != null)
        {
            var rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var tokenHash = _passwordService.HashPassword(rawToken);

            // Invalidate previous reset tokens
            var oldTokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var oldToken in oldTokens)
                oldToken.IsUsed = true;

            _context.PasswordResetTokens.Add(new Domain.Entities.PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            });

            // Also update User convenience fields
            user.PasswordResetToken = rawToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync(cancellationToken);

            _ = _emailService.SendPasswordResetEmailAsync(
                user.Email, user.FirstName, rawToken, CancellationToken.None);
        }

        return Result<ForgotPasswordResponseDto>.Success(new ForgotPasswordResponseDto
        {
            Message = "If an account with this email exists, a password reset link has been sent.",
            ResetTokenExpiresIn = 900
        });
    }
}

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}
