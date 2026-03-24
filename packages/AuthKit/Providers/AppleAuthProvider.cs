using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthKit.Options;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AuthKit.Providers;

public class AppleAuthProvider : ISocialAuthProvider
{
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string AppleJwksUrl = "https://appleid.apple.com/.well-known/openid-configuration";

    private readonly string _clientId;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configManager;

    public AppleAuthProvider(AppleOptions options)
    {
        _clientId = options.ClientId!;
        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            AppleJwksUrl,
            new OpenIdConnectConfigurationRetriever());
    }

    public SocialProvider Provider => SocialProvider.Apple;

    public async Task<SocialUserInfo> VerifyTokenAsync(string idToken, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _configManager.GetConfigurationAsync(cancellationToken);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AppleIssuer,
                ValidateAudience = true,
                ValidAudience = _clientId,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true
            };

            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(idToken, validationParams, out _);

            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw UnauthorizedException.TokenInvalid();

            var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
            var name = principal.FindFirstValue("name") ?? email;

            return new SocialUserInfo(sub, email, name);
        }
        catch (SecurityTokenException ex)
        {
            throw UnauthorizedException.TokenInvalid();
        }
    }
}
