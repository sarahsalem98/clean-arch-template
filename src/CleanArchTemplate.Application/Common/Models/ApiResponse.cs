using System.Text.Json.Serialization;

namespace CleanArchTemplate.Application.Common.Models;

/// <summary>Generic API response envelope.</summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; set; }
}

/// <summary>Error details inside an API response.</summary>
public class ApiError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Details { get; set; }
}

/// <summary>Static factory for producing ApiResponse envelopes.</summary>
public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string message = "Operation completed successfully")
        => new()
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow.ToString("o")
        };

    public static ApiResponse<object?> Fail(string code, string message, object? details = null)
        => new()
        {
            Success = false,
            Error = new ApiError { Code = code, Message = message, Details = details },
            Timestamp = DateTime.UtcNow.ToString("o")
        };

    public static ApiResponse<object?> ValidationFail(IDictionary<string, string[]> errors)
        => new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = ErrorCodes.ValidationError,
                Message = "Validation failed",
                Details = errors
            },
            Timestamp = DateTime.UtcNow.ToString("o")
        };

    public static ApiResponse<object?> RateLimitFail(int retryAfterSeconds = 60)
        => new()
        {
            Success = false,
            Error = new ApiError
            {
                Code = ErrorCodes.RateLimitExceeded,
                Message = "Too many requests. Please try again later.",
                Details = new { retryAfter = retryAfterSeconds }
            },
            Timestamp = DateTime.UtcNow.ToString("o")
        };
}
