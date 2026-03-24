using AuthKit.Options;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using Google.Apis.Auth;

namespace AuthKit.Providers;

public class GoogleAuthProvider : ISocialAuthProvider
{
    private readonly string _clientId;

    public GoogleAuthProvider(GoogleOptions options) =>
        _clientId = options.ClientId!;

    public SocialProvider Provider => SocialProvider.Google;

    public async Task<SocialUserInfo> VerifyTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_clientId]
                });

            var name = payload.Name
                ?? $"{payload.GivenName} {payload.FamilyName}".Trim();

            return new SocialUserInfo(payload.Subject, payload.Email, name);
        }
        catch (InvalidJwtException ex)
        {
            throw UnauthorizedException.TokenInvalid();
        }
    }
}
