namespace CleanArchTemplate.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string firstName, string verificationToken, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string toEmail, string firstName, string resetToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string firstName, CancellationToken cancellationToken = default);
    Task SendAccountDeletionNoticeAsync(string toEmail, string firstName, DateTime scheduledDeletionDate, CancellationToken cancellationToken = default);
}
