using CleanArchTemplate.Application.Common.Models;
using CleanArchTemplate.Domain.Exceptions;
using System.Text.Json;

namespace CleanArchTemplate.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";

        ApiResponse<object?> response;

        switch (exception)
        {
            case UnauthorizedException ue:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = ApiResponse.Fail(ue.ErrorCode, ue.Message);
                break;

            case ForbiddenException fe:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                response = ApiResponse.Fail(ForbiddenException.Code, fe.Message);
                break;

            case NotFoundException nfe:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response = ApiResponse.Fail(NotFoundException.Code, nfe.Message);
                break;

            case ConflictException ce:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response = ApiResponse.Fail(ConflictException.Code, ce.Message);
                break;

            case DomainValidationException dve:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                response = ApiResponse.Fail(DomainValidationException.Code, dve.Message);
                break;

            case RateLimitException rle:
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.RetryAfter = rle.RetryAfterSeconds.ToString();
                response = ApiResponse.RateLimitFail(rle.RetryAfterSeconds);
                break;

            case ServiceUnavailableException sue:
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                response = ApiResponse.Fail(ServiceUnavailableException.Code, sue.Message);
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = ApiResponse.Fail(ErrorCodes.InternalServerError, "An unexpected error occurred.");
                break;
        }

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}
