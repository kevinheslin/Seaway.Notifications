using Microsoft.EntityFrameworkCore;
using Seaway.Notifications.Data;
using Seaway.Notifications.Data.Entities;

namespace Seaway.Notifications.AdminUI.Services;

public class NotificationAdminService
{
    private readonly SeawayNotificationsDbContext _db;

    public NotificationAdminService(SeawayNotificationsDbContext db)
    {
        _db = db;
    }

    // Get all notification types for a given app
    public async Task<List<NotificationType>> GetNotificationTypesAsync(string appKey)
    {
        return await _db.NotificationTypes
            .Include(t => t.App)
            .Include(t => t.Subscriptions)
            .Where(t => t.App.AppKey == appKey)
            .OrderBy(t => t.DisplayName)
            .ToListAsync();
    }

    // Get a single notification type with its subscriptions
    public async Task<NotificationType?> GetNotificationTypeAsync(int id)
    {
        return await _db.NotificationTypes
            .Include(t => t.App)
            .Include(t => t.Subscriptions)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    // Add a new subscription
    public async Task AddSubscriptionAsync(
        int notificationTypeId,
        string channel,
        string recipient)
    {
        _db.Subscriptions.Add(new Subscription
        {
            NotificationTypeId = notificationTypeId,
            Channel = channel,
            Recipient = recipient,
            IsActive = true
        });

        await _db.SaveChangesAsync();
    }

    // Toggle a subscription active/inactive
    public async Task ToggleSubscriptionAsync(int subscriptionId)
    {
        var sub = await _db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) return;

        sub.IsActive = !sub.IsActive;
        await _db.SaveChangesAsync();
    }

    // Remove a subscription entirely
    public async Task RemoveSubscriptionAsync(int subscriptionId)
    {
        var sub = await _db.Subscriptions.FindAsync(subscriptionId);
        if (sub is null) return;

        _db.Subscriptions.Remove(sub);
        await _db.SaveChangesAsync();
    }
}
