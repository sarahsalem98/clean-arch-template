using CleanArchTemplate.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CleanArchTemplate.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehaviour(
        ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId?.ToString() ?? "Anonymous";

        _logger.LogInformation(
            "Handling {RequestName} for user {UserId}",
            requestName, userId);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms for user {UserId}",
                requestName, sw.ElapsedMilliseconds, userId);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex,
                "Error handling {RequestName} in {ElapsedMs}ms for user {UserId}",
                requestName, sw.ElapsedMilliseconds, userId);
            throw;
        }
    }
}
