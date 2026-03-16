namespace CleanArchTemplate.Domain.Exceptions;

public class UnauthorizedException : Exception
{
    public string ErrorCode { get; }

    public UnauthorizedException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public static UnauthorizedException InvalidCredentials()
        => new("AUTH_001", "Invalid credentials.");

    public static UnauthorizedException TokenExpired()
        => new("AUTH_002", "Token has expired.");

    public static UnauthorizedException TokenInvalid()
        => new("AUTH_003", "Token is invalid.");

    public static UnauthorizedException AccountNotVerified()
        => new("AUTH_004", "Account is not verified. Please verify your email.");

    public static UnauthorizedException AccountSuspended()
        => new("AUTH_005", "Account has been suspended. Please contact support.");

    public static UnauthorizedException AccountDeleted()
        => new("AUTH_006", "Account has been deleted.");
}
