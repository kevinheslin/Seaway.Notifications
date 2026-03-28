namespace Seaway.Notifications;

public class SeawayNotificationsOptions
{
    public string AppKey { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;

    public SmsOptions? Sms { get; set; }
    public TeamsOptions? Teams { get; set; }
    public PhoneOptions? Phone { get; set; }
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
