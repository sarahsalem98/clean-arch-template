namespace AuthKit.Options;

public class AuthKitOptions
{
    /// <summary>Google OAuth client ID. Set to enable Google sign-in.</summary>
    public string? GoogleClientId { get; set; }

    /// <summary>Apple sign-in settings. Set ClientId to enable Apple sign-in.</summary>
    public AppleOptions Apple { get; set; } = new();

    /// <summary>Facebook app credentials. Set AppId + AppSecret to enable Facebook sign-in.</summary>
    public FacebookOptions Facebook { get; set; } = new();

    public bool IsGoogleEnabled => !string.IsNullOrWhiteSpace(GoogleClientId);
    public bool IsAppleEnabled => !string.IsNullOrWhiteSpace(Apple.ClientId);
    public bool IsFacebookEnabled => !string.IsNullOrWhiteSpace(Facebook.AppId);
}

public class GoogleOptions
{
    public string? ClientId { get; set; }
}

public class AppleOptions
{
    public string? ClientId { get; set; }
    public string? TeamId { get; set; }
    public string? KeyId { get; set; }
    public string? PrivateKey { get; set; }
}

public class FacebookOptions
{
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }
}
