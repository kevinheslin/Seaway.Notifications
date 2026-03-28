namespace Seaway.Notifications.Data.Entities;

public class App
{
    public int Id { get; set; }
    public required string AppKey { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public ICollection<NotificationType> NotificationTypes { get; set; } = new List<NotificationType>();
}
