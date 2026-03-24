using CleanArchTemplate.Domain.Enums;
using CleanArchTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Infrastructure.BackgroundJobs;

public class PermanentDeletionJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PermanentDeletionJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public PermanentDeletionJob(IServiceScopeFactory scopeFactory, ILogger<PermanentDeletionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PermanentDeletionJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during permanent deletion job.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow.AddDays(-30);

        var usersToDelete = await context.Users
            .Where(u => u.Status == UserStatus.Deleted
                     && u.DeletionScheduledAt != null
                     && u.DeletionScheduledAt <= cutoff)
            .ToListAsync(cancellationToken);

        if (usersToDelete.Count == 0)
        {
            _logger.LogDebug("PermanentDeletionJob: no accounts to purge.");
            return;
        }

        foreach (var user in usersToDelete)
        {
            // Hard-delete related data cascade is handled by DB constraints/EF config
            context.Users.Remove(user);
            _logger.LogInformation("Permanently deleted user {UserId}.", user.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("PermanentDeletionJob: purged {Count} account(s).", usersToDelete.Count);
    }
}
