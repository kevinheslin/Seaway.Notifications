using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seaway.Mailer.Extensions;
using Seaway.Mailer;
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

        // Database
        services.AddDbContext<SeawayNotificationsDbContext>(db =>
            db.UseSqlServer(options.ConnectionString));

        // Email channel — always registered
        services.AddSeawayMailer(mailerOptions =>
        {
            mailerOptions.SmtpHost = options.Email.SmtpHost;
            mailerOptions.SmtpPort = options.Email.SmtpPort;
            mailerOptions.UseTls = options.Email.UseSsl;
            mailerOptions.FromAddress = options.Email.FromAddress;
            mailerOptions.FromDisplayName = options.Email.FromDisplayName;
            mailerOptions.SmtpUsername = options.Email.SmtpUsername;
            mailerOptions.SmtpPassword = options.Email.SmtpPassword;
        });

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

        // Dispatcher
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        return services;
    }
}
