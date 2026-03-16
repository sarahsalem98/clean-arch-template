namespace CleanArchTemplate.Application.Auth.DTOs;

public class AuthResponseDto
{
    public UserDto User { get; set; } = default!;
    public TokenDto Tokens { get; set; } = default!;
}

public class SocialAuthResponseDto
{
    public UserDto User { get; set; } = default!;
    public TokenDto Tokens { get; set; } = default!;
    public bool IsNewUser { get; set; }
    public string? Provider { get; set; }
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; } = 3600;
    public string TokenType { get; set; } = "Bearer";
}
