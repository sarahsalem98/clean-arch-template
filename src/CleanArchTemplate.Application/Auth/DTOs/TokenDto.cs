namespace CleanArchTemplate.Application.Auth.DTOs;

public class TokenDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; } = 3600;
    public string TokenType { get; set; } = "Bearer";
}
