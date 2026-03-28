namespace Seaway.Notifications.Data;

public class NotificationLog
{
    public int Id { get; set; }
    public required string AppKey { get; set; }
    public required string Channel { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ChannelResponse { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
