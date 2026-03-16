namespace CleanArchTemplate.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
