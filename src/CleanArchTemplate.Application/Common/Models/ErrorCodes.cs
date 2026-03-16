namespace CleanArchTemplate.Application.Common.Models;

/// <summary>Standard error codes used across all API responses.</summary>
public static class ErrorCodes
{
    // Authentication Errors
    public const string InvalidCredentials = "AUTH_001";
    public const string TokenExpired = "AUTH_002";
    public const string TokenInvalid = "AUTH_003";
    public const string AccountNotVerified = "AUTH_004";
    public const string AccountSuspended = "AUTH_005";
    public const string AccountDeleted = "AUTH_006";

    // Validation Errors
    public const string RequiredFieldMissing = "VAL_001";
    public const string InvalidFormat = "VAL_002";
    public const string ValueOutOfRange = "VAL_003";
    public const string InvalidFileType = "VAL_004";
    public const string FileTooLarge = "VAL_005";
    public const string ValidationError = "VALIDATION_ERROR";

    // Resource Errors
    public const string ResourceNotFound = "RES_001";
    public const string ResourceAlreadyExists = "RES_002";
    public const string ResourceAccessDenied = "RES_003";

    // Server Errors
    public const string InternalServerError = "SRV_001";
    public const string ServiceUnavailable = "SRV_002";
    public const string DatabaseConnectionFailed = "SRV_003";

    // Rate Limiting
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
}
