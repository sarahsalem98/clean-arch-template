namespace CleanArchTemplate.Application.Users.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = default!;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; }
}
