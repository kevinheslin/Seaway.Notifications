namespace Seaway.Notifications.Data.Entities;

public class NotificationType
{
    public int Id { get; set; }
    public required string TypeKey { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public int AppId { get; set; }
    public App App { get; set; } = null!;
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
