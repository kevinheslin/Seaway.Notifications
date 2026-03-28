using Seaway.Notifications.Abstractions;

namespace Seaway.Notifications.Channels;

internal interface INotificationChannel
{
    string ChannelName { get; }
    Task<(bool Success, string Response)> SendAsync(string recipient, NotificationContext context);
}
