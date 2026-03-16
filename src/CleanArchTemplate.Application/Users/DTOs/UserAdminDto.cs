namespace CleanArchTemplate.Application.Users.DTOs;

public class UserAdminDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool IsVerified { get; set; }
    public string Status { get; set; } = default!;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
