# AuthKit

Complete authentication and authorization kit for ASP.NET Core. Includes JWT, password hashing, RBAC, and social login (Google, Apple, Facebook).

## Installation

```bash
dotnet add package AuthKit
```

## Quick Start

### 1. `appsettings.json`

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-minimum-32-characters",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers"
  }
}
```

### 2. `Program.cs`

```csharp
builder.Services.AddAuthKit(builder.Configuration, options =>
{
    // Social providers are optional — only enable what you need
    options.GoogleClientId = builder.Configuration["Google:ClientId"];

    options.Apple.ClientId   = builder.Configuration["Apple:ClientId"];
    options.Apple.TeamId     = builder.Configuration["Apple:TeamId"];
    options.Apple.KeyId      = builder.Configuration["Apple:KeyId"];
    options.Apple.PrivateKey = builder.Configuration["Apple:PrivateKey"];

    options.Facebook.AppId     = builder.Configuration["Facebook:AppId"];
    options.Facebook.AppSecret = builder.Configuration["Facebook:AppSecret"];
});

// Register your DbContext as IApplicationDbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<AppDbContext>());

// Register your email service implementation
builder.Services.AddScoped<IEmailService, YourEmailService>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### 3. `AppDbContext.cs`

```csharp
public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<SocialLogin> SocialLogins => Set<SocialLogin>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddAuthKitEntities();
    }
}
```

### 4. `AuthController.cs`

```csharp
using CleanArchTemplate.Application.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized();
    }

    [HttpPost("social-login")]
    public async Task<IActionResult> SocialLogin(SocialLoginCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : Unauthorized();
    }

    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var result = await sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

## What You Must Implement

AuthKit requires two implementations from your project:

### `IEmailService`

```csharp
public class YourEmailService : IEmailService
{
    public Task SendVerificationEmailAsync(string email, string name, string token, CancellationToken ct = default)
    {
        // Send with SendGrid, SMTP, etc.
    }

    public Task SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken ct = default)
    {
        // Your implementation
    }

    public Task SendWelcomeEmailAsync(string email, string name, CancellationToken ct = default)
    {
        // Your implementation
    }

    public Task SendAccountDeletionNoticeAsync(string email, string name, DateTime scheduledDate, CancellationToken ct = default)
    {
        // Your implementation
    }
}
```

### `IApplicationDbContext`

Your EF Core `DbContext` must implement this interface (see step 3 above).

## Available Commands

| Command | Description |
|---|---|
| `LoginCommand` | Email/password login → JWT + refresh token |
| `RegisterCommand` | Create account → JWT + refresh token |
| `RefreshTokenCommand` | Rotate refresh token → new JWT |
| `SocialLoginCommand` | Google / Apple / Facebook login |
| `LogoutCommand` | Revoke refresh token |
| `ChangePasswordCommand` | Change authenticated user's password |
| `ForgotPasswordCommand` | Send password reset email |
| `ResetPasswordCommand` | Reset password with token |
| `VerifyEmailCommand` | Verify email with token |

## Database Tables

`AddAuthKitEntities()` configures these tables via EF Core Fluent API:

- `Users`, `Roles`, `Permissions`
- `UserRoles`, `RolePermissions`
- `RefreshTokens`, `DeviceTokens`
- `SocialLogins`, `PasswordResetTokens`
- `AuditLogs`

## License

MIT
