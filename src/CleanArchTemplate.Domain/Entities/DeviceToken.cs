namespace CleanArchTemplate.Domain.Entities;

public class DeviceToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Token { get; set; } = default!;
    public string DeviceType { get; set; } = default!;
    public DateTime? LastUsedAt { get; set; }
}
