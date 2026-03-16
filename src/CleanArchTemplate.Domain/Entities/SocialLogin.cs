using CleanArchTemplate.Domain.Enums;

namespace CleanArchTemplate.Domain.Entities;

public class SocialLogin : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public SocialProvider Provider { get; set; }
    public string ProviderUserId { get; set; } = default!;
    public string? ProviderEmail { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
}
