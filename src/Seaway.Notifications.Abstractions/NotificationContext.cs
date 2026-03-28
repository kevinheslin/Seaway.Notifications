namespace Seaway.Notifications.Abstractions;

public class NotificationContext
{
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
