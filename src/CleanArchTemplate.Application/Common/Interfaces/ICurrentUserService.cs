namespace CleanArchTemplate.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Portal { get; }
    string? JwtId { get; }
    IList<string> Roles { get; }
    IList<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
