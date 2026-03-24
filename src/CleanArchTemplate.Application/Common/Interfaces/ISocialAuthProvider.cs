using CleanArchTemplate.Domain.Enums;

namespace CleanArchTemplate.Application.Common.Interfaces;

public record SocialUserInfo(string ProviderUserId, string Email, string Name);

public interface ISocialAuthProvider
{
    SocialProvider Provider { get; }
    Task<SocialUserInfo> VerifyTokenAsync(string token, CancellationToken cancellationToken);
}
