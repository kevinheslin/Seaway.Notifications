using System.Threading.Channels;

namespace Seaway.Notifications;

public class SeawayNotificationsOptions
{
    public string AppKey { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;

    public EmailOptions Email { get; set; } = new();
    public SmsOptions? Sms { get; set; }
    public TeamsOptions? Teams { get; set; }
    public PhoneOptions? Phone { get; set; }
}

public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 25;
    public bool UseSsl { get; set; } = false;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "Seaway Notifications";
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
}

public class SmsOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class TeamsOptions
{
    public string WebhookUrl { get; set; } = string.Empty;
}

public class PhoneOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

