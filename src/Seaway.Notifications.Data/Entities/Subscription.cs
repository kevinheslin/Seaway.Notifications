namespace Seaway.Notifications.Data.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public required string Channel { get; set; }   // "Email" | "Sms" | "Teams" | "Phone"
    public required string Recipient { get; set; } // email address, phone number, or webhook URL
    public bool IsActive { get; set; } = true;
}