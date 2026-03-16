namespace CleanArchTemplate.Application.Auth.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? ThumbnailImage { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
