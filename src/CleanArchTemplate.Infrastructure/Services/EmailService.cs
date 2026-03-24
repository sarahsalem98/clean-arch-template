using CleanArchTemplate.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchTemplate.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(
        string toEmail, string firstName, string verificationToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Verification email to {Email} — token: {Token}",
            toEmail, verificationToken);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(
        string toEmail, string firstName, string resetToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Password-reset email to {Email} — token: {Token}",
            toEmail, resetToken);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(
        string toEmail, string firstName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Welcome email to {Email} ({Name})",
            toEmail, firstName);
        return Task.CompletedTask;
    }

    public Task SendAccountDeletionNoticeAsync(
        string toEmail, string firstName, DateTime scheduledDeletionDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Deletion notice to {Email} — scheduled: {Date}",
            toEmail, scheduledDeletionDate.ToString("O"));
        return Task.CompletedTask;
    }
}
