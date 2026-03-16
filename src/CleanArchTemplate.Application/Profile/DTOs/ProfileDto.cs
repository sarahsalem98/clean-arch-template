using CleanArchTemplate.Domain.ValueObjects;

namespace CleanArchTemplate.Application.Profile.DTOs;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? ThumbnailImage { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Address? Address { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProfileImageResponseDto
{
    public string ImageUrl { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
}
