using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Application.Profile.DTOs;
using CleanArchTemplate.Domain.Exceptions;
using CleanArchTemplate.Domain.ValueObjects;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Profile.Commands;

public record UpdateProfileCommand : IRequest<Result<ProfileDto>>
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Phone { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public AddressDto? Address { get; init; }
}

public class AddressDto
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw UnauthorizedException.TokenInvalid();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        if (request.FirstName != null)
            user.FirstName = request.FirstName.Trim();

        if (request.LastName != null)
            user.LastName = request.LastName.Trim();

        if (request.Phone != null)
            user.Phone = request.Phone.Trim();

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth;

        if (request.Gender != null)
        {
            if (Enum.TryParse<Domain.Enums.Gender>(request.Gender, true, out var gender))
                user.Gender = gender;
        }

        if (request.Address != null)
        {
            user.Address = new Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                Country = request.Address.Country,
                PostalCode = request.Address.PostalCode
            };
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = user.Adapt<ProfileDto>();
        return Result<ProfileDto>.Success(dto);
    }
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Phone)
            .Matches(@"^\+[1-9]\d{1,14}$").WithMessage("Phone must be in E.164 format.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.DateOfBirth)
            .Must(dob => !dob.HasValue || dob.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth must be in the past.")
            .Must(dob => !dob.HasValue || CalculateAge(dob.Value) >= 13)
                .WithMessage("User must be at least 13 years old.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .Must(g => g == null || g == "male" || g == "female" || g == "other")
            .WithMessage("Gender must be 'male', 'female', or 'other'.");
    }

    private static int CalculateAge(DateOnly dob)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }
}
