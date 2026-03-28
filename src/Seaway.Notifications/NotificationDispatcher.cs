using Microsoft.EntityFrameworkCore;
using Seaway.Notifications.Abstractions;
using Seaway.Notifications.Channels;
using Seaway.Notifications.Data;
using Seaway.Notifications.Data.Entities;
using System.Diagnostics.Metrics;
using System.Threading.Channels;
using static System.Net.WebRequestMethods;

namespace Seaway.Notifications;

internal sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly SeawayNotificationsDbContext _db;
    private readonly IEnumerable<INotificationChannel> _channels;

    public NotificationDispatcher(
        SeawayNotificationsDbContext db,
        IEnumerable<INotificationChannel> channels)
    {
        _db = db;
        _channels = channels;
    }

    public async Task<NotificationResult> SendAsync(
        string notificationTypeKey,
        NotificationContext context)
    {
        var result = new NotificationResult();

        // Look up active subscriptions for this notification type
        var subscriptions = await _db.Subscriptions
            .Include(s => s.NotificationType)
            .Where(s => s.NotificationType.TypeKey == notificationTypeKey
                     && s.IsActive)
            .ToListAsync();

        if (!subscriptions.Any())
            return result;

        // Dispatch to each subscription in parallel
        var tasks = subscriptions.Select(async sub =>
        {
            var channel = _channels.FirstOrDefault(c =>
                c.ChannelName.Equals(sub.Channel, StringComparison.OrdinalIgnoreCase));

            if (channel is null)
            {
                result.Errors.Add($"No channel implementation found for '{sub.Channel}'.");
                return;
            }

            var success = await channel.SendAsync(sub.Recipient, context);

            // Log the attempt
            _db.NotificationLogs.Add(new NotificationLog
            {
                NotificationTypeId = sub.NotificationTypeId,
                Channel = sub.Channel,
                Recipient = sub.Recipient,
                Subject = context.Subject,
                Body = context.Body,
                Success = success,
                ErrorMessage = success ? null : $"Channel '{sub.Channel}' returned failure."
            });

            if (success)
                result.Sent++;
            else
                result.Failed++;
        });

        await Task.WhenAll(tasks);
        await _db.SaveChangesAsync();

        return result;
    }
}
