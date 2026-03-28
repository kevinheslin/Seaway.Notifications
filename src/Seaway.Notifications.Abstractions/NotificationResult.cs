namespace Seaway.Notifications.Abstractions;

public class NotificationResult
{
    public int Sent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
