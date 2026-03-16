namespace CleanArchTemplate.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int HttpStatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T value, int httpStatusCode = 200)
        => new() { IsSuccess = true, Value = value, HttpStatusCode = httpStatusCode };

    public static Result<T> Failure(string errorCode, string message, int httpStatusCode)
        => new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = message, HttpStatusCode = httpStatusCode };

    public static Result<T> NotFound(string message = "Resource not found.")
        => Failure(ErrorCodes.ResourceNotFound, message, 404);

    public static Result<T> Conflict(string message = "Resource already exists.")
        => Failure(ErrorCodes.ResourceAlreadyExists, message, 409);

    public static Result<T> Unauthorized(string code, string message)
        => Failure(code, message, 401);

    public static Result<T> Forbidden(string message = "Access denied.")
        => Failure(ErrorCodes.ResourceAccessDenied, message, 403);
}
