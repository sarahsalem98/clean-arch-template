# CleanArchTemplate

A production-ready ASP.NET Core 10 Web API built on **Clean Architecture** principles, featuring JWT authentication, role-based access control (RBAC), multi-portal support, and a fully scaffolded feature set.

---

## Architecture Overview

```
CleanArchTemplate/
├── src/
│   ├── CleanArchTemplate.Domain          # Entities, enums, value objects, exceptions
│   ├── CleanArchTemplate.Application     # CQRS (MediatR), validators, interfaces, DTOs
│   ├── CleanArchTemplate.Infrastructure  # EF Core, JWT, BCrypt, background jobs, seeders
│   └── CleanArchTemplate.WebAPI          # Controllers, middleware, authorization, Swagger
└── tests/
    ├── CleanArchTemplate.UnitTests       # Handler unit tests (Moq + xUnit + FluentAssertions)
    └── CleanArchTemplate.IntegrationTests# WebApplicationFactory API tests
```

### Dependency flow (inner → outer)

```
Domain  ←  Application  ←  Infrastructure
                         ↖  WebAPI
```

Domain and Application have **no** external package dependencies (except MediatR / FluentValidation abstractions).

---

## Features

| Feature | Details |
|---|---|
| Authentication | JWT Bearer — access token (1 h) + refresh token (30 days, rotated) |
| Multi-portal | `user-portal` (mobile/web) and `admin-portal` — portal claim enforced per route area |
| RBAC | Roles (`SuperAdmin`, `Admin`, `User`) + granular permissions, both embedded in JWT |
| Social login | Google / Apple / Facebook stub ready for provider token validation |
| Email verification | Token-based, 24-hour expiry |
| Password reset | Token-based, email delivery stub |
| Profile management | Update, image upload (5 MB, JPEG/PNG/WEBP), soft-delete with 30-day grace period |
| Rate limiting | Sliding-window — 100 req/60 s global, 10 req/60 s on auth endpoints |
| CORS | Configured via `appsettings.json` |
| Compression | GZIP + Brotli via `ResponseCompression` |
| API versioning | URL segment (`/api/v1/…`) + `X-Api-Version` header |
| Swagger | Interactive docs at `/swagger` in Development |
| Audit logging | Every significant action recorded in `AuditLogs` table |
| Background job | `PermanentDeletionJob` — runs every 24 h, hard-deletes accounts past their 30-day grace period |
| Serilog | Console + rolling-file sinks, configurable via `appsettings` |

---

## How to Run

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB is fine for development)

### 1 — Configure

Copy `appsettings.Development.json` and update:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CleanArchTemplateDb_Dev;..."
  },
  "Jwt": {
    "Secret": "your-secret-at-least-32-characters-long!"
  }
}
```

> **Never** commit real secrets to source control.  Use environment variables or a secrets manager in production.

### 2 — Migrate & seed

Migrations are applied automatically on startup.  To apply manually:

```bash
cd src/CleanArchTemplate.WebAPI
dotnet ef database update --project ../CleanArchTemplate.Infrastructure
```

The seeder creates:
- Roles: `SuperAdmin`, `Admin`, `User`
- Permissions for each role
- A default `SuperAdmin` account — `superadmin@example.com` / `SuperAdmin@123` *(change immediately in production)*

### 3 — Run

```bash
dotnet run --project src/CleanArchTemplate.WebAPI
```

Navigate to `https://localhost:{port}/swagger` to explore the API.

---

## API Reference

### User Portal (`/api/v1/user/…`)

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | — | Create account |
| POST | `/auth/login` | — | Email/password login |
| POST | `/auth/refresh-token` | — | Rotate refresh token |
| POST | `/auth/forgot-password` | — | Request reset email |
| POST | `/auth/reset-password` | — | Reset with token |
| POST | `/auth/change-password` | Bearer | Change password |
| POST | `/auth/logout` | Bearer | Revoke tokens |
| POST | `/auth/verify-email` | — | Verify email token |
| POST | `/auth/social-login` | — | Social provider login |
| GET  | `/profile` | Bearer | Get own profile |
| PUT  | `/profile` | Bearer | Update profile |
| POST | `/profile/image` | Bearer | Upload profile image |
| DELETE | `/profile` | Bearer | Schedule account deletion |

### Admin Portal (`/api/v1/admin/…`)

| Method | Path | Permission | Description |
|---|---|---|---|
| POST | `/auth/login` | — | Admin login |
| POST | `/auth/refresh-token` | — | Rotate token |
| POST | `/auth/logout` | Bearer | Logout |
| GET  | `/users` | `users.view` | Paginated user list |
| GET  | `/users/{id}` | `users.view` | Get user by ID |
| PATCH | `/users/{id}/status` | `users.status.update` | Change user status |
| POST | `/users/{id}/roles` | `users.role.assign` | Assign role |
| GET  | `/users/audit-logs` | `audit-logs.view` | Audit logs |

All admin routes require a JWT with `portal=admin-portal`.

---

## How to Add a New Portal

1. Add a new route area under `Areas/YourPortal/Controllers/`.
2. Register the portal path prefix in `PortalValidationMiddleware.cs`.
3. Add a portal policy in `DependencyInjection.cs` (`Portal:your-portal`).
4. Issue access tokens with `portal = "your-portal"` in the relevant login command.

## How to Add a New Permission

1. Add the permission string to `DataSeeder.cs` and map it to roles.
2. Register the policy in `WebAPI/DependencyInjection.cs`:
   ```csharp
   options.AddPolicy("Permission:your.permission", policy =>
       policy.Requirements.Add(new PermissionRequirement("your.permission")));
   ```
3. Decorate your controller action:
   ```csharp
   [RequirePermission("your.permission")]
   ```

---

## Response Envelope

Every response — success or error — uses the same `ApiResponse<T>` envelope:

```jsonc
// Success
{
  "success": true,
  "data": { ... },
  "message": "Operation completed successfully.",
  "timestamp": "2026-03-17T10:00:00.000Z"
}

// Error
{
  "success": false,
  "error": {
    "code": "AUTH_001",
    "message": "Invalid credentials.",
    "details": null          // validation errors appear here
  },
  "timestamp": "2026-03-17T10:00:00.000Z"
}
```

### Standard error codes

| Code | Meaning |
|---|---|
| `AUTH_001` | Invalid credentials |
| `AUTH_002` | Token expired |
| `AUTH_003` | Token invalid |
| `AUTH_004` | Account not verified |
| `AUTH_005` | Account suspended |
| `AUTH_006` | Account deleted |
| `VAL_001` – `VAL_005` | Validation errors |
| `RES_001` | Not found |
| `RES_002` | Conflict / already exists |
| `RES_003` | Forbidden |
| `SRV_001` | Internal server error |
| `SRV_002` | Service unavailable |
| `RATE_LIMIT_EXCEEDED` | Too many requests |
| `VALIDATION_ERROR` | FluentValidation failure |

---

## Response Time Targets

| Operation | Target p95 |
|---|---|
| Login / Register | < 200 ms |
| Token refresh | < 50 ms |
| Profile read | < 30 ms |
| Admin user list (paginated) | < 100 ms |
| Image upload | < 500 ms |

---

## Running Tests

```bash
# Unit tests
dotnet test tests/CleanArchTemplate.UnitTests

# Integration tests (requires SQL Server or Docker)
dotnet test tests/CleanArchTemplate.IntegrationTests
```
