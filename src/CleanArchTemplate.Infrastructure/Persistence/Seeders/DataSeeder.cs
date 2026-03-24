using CleanArchTemplate.Domain.Entities;
using CleanArchTemplate.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Infrastructure.Persistence.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        await SeedRolesAsync(context, logger);
        await SeedPermissionsAsync(context, logger);
        await SeedRolePermissionsAsync(context, logger);
        await SeedSuperAdminAsync(context, logger);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, ILogger logger)
    {
        var roles = new[]
        {
            new { Name = "SuperAdmin", Description = "Full system access" },
            new { Name = "Admin",      Description = "Administrative access" },
            new { Name = "User",       Description = "Standard user access" },
        };

        foreach (var r in roles)
        {
            if (!await context.Roles.AnyAsync(x => x.Name == r.Name))
            {
                context.Roles.Add(new Role { Name = r.Name, Description = r.Description });
                logger.LogInformation("Seeded role: {Role}", r.Name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        var permissions = new[]
        {
            // User-portal permissions
            "profile.view",
            "profile.edit",
            "profile.image.upload",
            "account.delete",
            // Admin-portal permissions
            "users.view",
            "users.status.update",
            "users.role.assign",
            "audit-logs.view",
        };

        foreach (var name in permissions)
        {
            if (!await context.Permissions.AnyAsync(p => p.Name == name))
            {
                context.Permissions.Add(new Permission { Name = name });
                logger.LogInformation("Seeded permission: {Permission}", name);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext context, ILogger logger)
    {
        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["SuperAdmin"] = [
                "profile.view", "profile.edit", "profile.image.upload", "account.delete",
                "users.view", "users.status.update", "users.role.assign", "audit-logs.view"
            ],
            ["Admin"] = [
                "profile.view", "profile.edit", "profile.image.upload",
                "users.view", "users.status.update", "audit-logs.view"
            ],
            ["User"] = [
                "profile.view", "profile.edit", "profile.image.upload", "account.delete"
            ],
        };

        foreach (var (roleName, permNames) in rolePermissionMap)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) continue;

            foreach (var permName in permNames)
            {
                var perm = await context.Permissions.FirstOrDefaultAsync(p => p.Name == permName);
                if (perm == null) continue;

                var exists = await context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);

                if (!exists)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = perm.Id
                    });
                }
            }
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded role-permission mappings.");
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context, ILogger logger)
    {
        const string superAdminEmail = "superadmin@example.com";

        if (await context.Users.AnyAsync(u => u.Email == superAdminEmail))
            return;

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123", workFactor: 12);

        var user = new User
        {
            Email = superAdminEmail,
            PasswordHash = passwordHash,
            FirstName = "Super",
            LastName = "Admin",
            IsVerified = true,
            Status = UserStatus.Active,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole != null)
        {
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = superAdminRole.Id });
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Seeded SuperAdmin user: {Email}", superAdminEmail);
    }
}
