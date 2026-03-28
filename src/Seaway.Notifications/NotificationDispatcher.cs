using Microsoft.EntityFrameworkCore;
using Seaway.Notifications.Abstractions;
using Seaway.Notifications.Channels;
using Seaway.Notifications.Data;

namespace Seaway.Notifications;

internal sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly SeawayNotificationsDbContext _db;
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly string _appKey;

    public NotificationDispatcher(
        SeawayNotificationsDbContext db,
        IEnumerable<INotificationChannel> channels,
        string appKey)
    {
        _db = db;
        _channels = channels;
        _appKey = appKey;
    }

    public async Task<NotificationResult> SendAsync(
        string channel,
        string recipient,
        NotificationContext context)
    {
        var result = new NotificationResult();

        var channelImpl = _channels.FirstOrDefault(c =>
            c.ChannelName.Equals(channel, StringComparison.OrdinalIgnoreCase));

        if (channelImpl is null)
        {
            result.Failed++;
            result.Errors.Add($"No channel implementation found for '{channel}'.");
            return result;
        }

        var (success, channelResponse) = await channelImpl.SendAsync(recipient, context);

        _db.NotificationLogs.Add(new Data.NotificationLog
        {
            AppKey = _appKey,
            Channel = channel,
            Recipient = recipient,
            Subject = context.Subject,
            Success = success,
            ErrorMessage = success ? null : channelResponse,
            ChannelResponse = channelResponse,
            SentAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        if (success) result.Sent++;
        else result.Failed++;

        return result;
    }
}
