using System.Text;
using static System.Net.WebRequestMethods;

namespace Seaway.Notifications.Data.Entities;

public class NotificationLog
{
    public int Id { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public required string Channel { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public string? Body { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
