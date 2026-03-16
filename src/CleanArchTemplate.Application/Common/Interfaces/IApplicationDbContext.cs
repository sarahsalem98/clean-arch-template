using CleanArchTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchTemplate.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<DeviceToken> DeviceTokens { get; }
    DbSet<SocialLogin> SocialLogins { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
