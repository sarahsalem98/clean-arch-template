using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Auth.Commands;

public record ChangePasswordCommand : IRequest<Result<object?>>
{
    public string CurrentPassword { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
    public string ConfirmPassword { get; init; } = default!;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<object?>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(
        IApplicationDbContext context,
        IPasswordService passwordService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordService = passwordService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<object?>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new DomainValidationException("confirmPassword", "Passwords do not match.");

        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.TokenInvalid();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new DomainValidationException("currentPassword", "Current password is incorrect.");

        user.PasswordHash = _passwordService.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

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
