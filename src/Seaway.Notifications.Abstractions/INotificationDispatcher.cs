namespace Seaway.Notifications.Abstractions;

public interface INotificationDispatcher
{
    Task<NotificationResult> SendAsync(string notificationTypeKey, NotificationContext context);
}
