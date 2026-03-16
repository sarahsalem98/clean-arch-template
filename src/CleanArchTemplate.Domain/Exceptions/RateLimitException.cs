namespace CleanArchTemplate.Domain.Exceptions;

public class RateLimitException : Exception
{
    public const string Code = "RATE_LIMIT_EXCEEDED";
    public int RetryAfterSeconds { get; }

    public RateLimitException(int retryAfterSeconds = 60)
        : base("Too many requests. Please try again later.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
