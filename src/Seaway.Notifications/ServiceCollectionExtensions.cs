using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seaway.Notifications.Abstractions;
using Seaway.Notifications.Channels;
using Seaway.Notifications.Data;

namespace Seaway.Notifications;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeawayNotifications(
        this IServiceCollection services,
        Action<SeawayNotificationsOptions> configure)
    {
        var options = new SeawayNotificationsOptions();
        configure(options);

        // Database — log only
        services.AddDbContext<SeawayNotificationsDbContext>(db =>
            db.UseSqlServer(options.ConnectionString));

        // Email channel — IAlertMailer is already registered by AddSeawayMailer()
        // in the consuming app. We resolve it from DI.
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();

        // SMS channel — only if configured
        if (options.Sms is not null)
        {
            services.AddScoped<INotificationChannel>(_ =>
                new SmsNotificationChannel(
                    options.Sms.AccountSid,
                    options.Sms.AuthToken,
                    options.Sms.FromNumber));
        }

        // Teams channel — only if configured
        if (options.Teams is not null)
        {
            services.AddScoped<INotificationChannel>(_ =>
                new TeamsNotificationChannel(options.Teams.WebhookUrl));
        }

        // Phone channel — only if configured
        if (options.Phone is not null)
        {
            services.AddScoped<INotificationChannel>(_ =>
                new PhoneNotificationChannel(
                    options.Phone.AccountSid,
                    options.Phone.AuthToken,
                    options.Phone.FromNumber));
        }

        // Dispatcher — pass AppKey through as a scalar
        services.AddScoped<INotificationDispatcher>(sp =>
            new NotificationDispatcher(
                sp.GetRequiredService<SeawayNotificationsDbContext>(),
                sp.GetRequiredService<IEnumerable<INotificationChannel>>(),
                options.AppKey));

        return services;
    }
}
