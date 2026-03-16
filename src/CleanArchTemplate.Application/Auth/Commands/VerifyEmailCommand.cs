using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record VerifyEmailCommand : IRequest<Result<object?>>
{
    public string Token { get; init; } = default!;
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;

    public VerifyEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<object?>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.EmailVerificationToken == request.Token &&
                !u.IsDeleted, cancellationToken);

        if (user == null)
            throw new DomainValidationException("token", "Verification token is invalid.");

        if (user.EmailVerificationTokenExpiry.HasValue &&
            user.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
            throw new DomainValidationException("token", "Verification token has expired.");

        if (user.IsVerified)
            return Result<object?>.Success(null); // Already verified — idempotent

        user.IsVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}
