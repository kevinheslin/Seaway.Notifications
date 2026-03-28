namespace Seaway.Notifications.Abstractions;

public interface INotificationDispatcher
{
    Task<NotificationResult> SendAsync(
        string channel,
        string recipient,
        NotificationContext context);
}
