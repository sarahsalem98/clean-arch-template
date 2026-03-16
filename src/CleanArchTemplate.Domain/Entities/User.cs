using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.ValueObjects;

namespace CleanArchTemplate.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public string? ThumbnailImage { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public Address? Address { get; set; }
    public bool IsVerified { get; set; } = false;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? DeletionScheduledAt { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public ICollection<SocialLogin> SocialLogins { get; set; } = new List<SocialLogin>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public string FullName => $"{FirstName} {LastName}";
}
