using System.Net.Http.Json;
using AuthKit.Options;
using CleanArchTemplate.Application.Common.Interfaces;
using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Domain.Exceptions;

namespace AuthKit.Providers;

public class FacebookAuthProvider : ISocialAuthProvider
{
    private readonly string _appId;
    private readonly string _appSecret;
    private readonly HttpClient _httpClient;

    public FacebookAuthProvider(FacebookOptions options, IHttpClientFactory factory)
    {
        _appId = options.AppId!;
        _appSecret = options.AppSecret!;
        _httpClient = factory.CreateClient("AuthKit.Facebook");
    }

    public SocialProvider Provider => SocialProvider.Facebook;

    public async Task<SocialUserInfo> VerifyTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        // Step 1: Validate token via debug_token
        var appToken = $"{_appId}|{_appSecret}";
        var debugUrl = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appToken}";

        var debugResponse = await _httpClient.GetFromJsonAsync<FacebookDebugResponse>(debugUrl, cancellationToken)
            ?? throw UnauthorizedException.TokenInvalid();

        if (!debugResponse.Data.IsValid)
            throw UnauthorizedException.TokenInvalid();

        // Step 2: Fetch user profile
        var profileUrl = $"https://graph.facebook.com/me?fields=id,email,name&access_token={accessToken}";
        var profile = await _httpClient.GetFromJsonAsync<FacebookProfile>(profileUrl, cancellationToken)
            ?? throw UnauthorizedException.TokenInvalid();

        return new SocialUserInfo(
            profile.Id,
            profile.Email ?? string.Empty,
            profile.Name ?? string.Empty);
    }

    private record FacebookDebugResponse(FacebookDebugData Data);
    private record FacebookDebugData(bool IsValid);
    private record FacebookProfile(string Id, string? Email, string? Name);
}
