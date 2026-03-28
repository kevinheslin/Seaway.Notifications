using Seaway.Mailer;
using Seaway.Notifications.Abstractions;

namespace Seaway.Notifications.Channels;

internal sealed class EmailNotificationChannel : INotificationChannel
{
    private readonly IAlertMailer _mailer;

    public string ChannelName => "Email";

    public EmailNotificationChannel(IAlertMailer mailer)
    {
        _mailer = mailer;
    }

    public async Task<(bool Success, string Response)> SendAsync(
        string recipient,
        NotificationContext context)
    {
        try
        {
            await _mailer.SendAlertAsync(new AlertMailMessage
            {
                To = [new MailRecipient(recipient)],
                Title = context.Subject,
                Detail = context.Body,
                Severity = context.Severity == NotificationSeverity.Critical
                               ? AlertSeverity.Critical
                               : context.Severity == NotificationSeverity.Warning
                                   ? AlertSeverity.Warning
                                   : AlertSeverity.Error,
            });

            return (true, "Email sent successfully.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}