using CleanArchTemplate.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AuthKit.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Registers all AuthKit entity configurations (User, Role, Permission, RefreshToken, etc.)
    /// Call this inside your DbContext's OnModelCreating.
    /// </summary>
    public static ModelBuilder AddAuthKitEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceTokenConfiguration());
        modelBuilder.ApplyConfiguration(new SocialLoginConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        return modelBuilder;
    }
}
