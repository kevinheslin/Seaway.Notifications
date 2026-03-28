# CLAUDE_CONTEXT_Notifications_v1.0.md

**Project:** Seaway.Notifications  
**Status:** v1.0.1 published to seaway-internal feed. Architecture finalized as delivery service model.  
**Repo:** github.com/kevinheslin/Seaway.Notifications (private)  
**Last updated:** March 28, 2026

---

## Purpose

Seaway.Notifications is a multi-channel notification **delivery service**. It accepts a channel, a recipient, and a message — and delivers it. It has no knowledge of notification types, trigger conditions, or recipient lists. Those are the responsibility of each consuming application.

---

## Architecture

### What Seaway.Notifications owns
- Channel implementations: Email (via Seaway.Mailer IAlertMailer), SMS (Twilio), Teams (Incoming Webhook), Phone (Twilio Voice)
- Transport-level logging — `NotificationLogs` table in `SeawayNotifications` database on SPC-DB01
- `NotificationChannel` constants — the only valid channel name strings
- `INotificationDispatcher.SendAsync(channel, recipient, context)` — the single public entry point

### What each consuming app owns
- `NotificationTypes` entity — the events that can trigger notifications
- `NotificationSubscriptions` entity — who gets notified on what channel per type
- `NotificationLog` entity — business-level record of what fired, who was notified, result
- Logic to query subscriptions at trigger time and call SendAsync per subscription
- Admin UI for managing subscriptions
- AppSecurity role authorization protecting the Admin UI

### Key architectural decision
The consuming app resolves recipients. Seaway.Notifications never knows who the recipient is until SendAsync is called. This means the library never needs to change when an app adds new notification types, changes recipients, or adds trigger conditions.

---

## Solution Structure

```
Seaway.Notifications/
  src/
    Seaway.Notifications.Abstractions/   — interfaces, enums, NotificationChannel constants
    Seaway.Notifications.Data/           — NotificationLog entity, DbContext, migration
    Seaway.Notifications/                — dispatcher, 4 channel implementations, DI registration
```

**Note:** AdminUI project was removed in v1.0.1 refactor. Each consuming app builds its own Admin UI.

---

## Database

**SeawayNotifications on SPC-DB01** — single table:

```
NotificationLogs
  Id              int PK
  AppKey          nvarchar(50)   — which app sent it
  Channel         nvarchar(50)   — Email | Sms | Teams | Phone
  Recipient       nvarchar(500)  — email, phone, webhook URL
  Subject         nvarchar(500)
  Success         bit
  ErrorMessage    nvarchar(2000)
  ChannelResponse nvarchar(2000) — raw SMTP reply, Twilio SID, HTTP status
  SentAt          datetime2
```

Connection string: `Server=SPC-DB01;Database=SeawayNotifications;Integrated Security=True;TrustServerCertificate=True;`

**Important:** `TrustServerCertificate=True` is required. SPC-DB01 uses a self-signed certificate.

---

## NuGet Packages (seaway-internal feed)

| Package | Version |
|---------|---------|
| Seaway.Notifications.Abstractions | 1.0.1 |
| Seaway.Notifications | 1.0.1 |

**Note:** v1.0.0 packages exist on the feed but are the old subscription platform model. Consuming apps must reference v1.0.1.

---

## Key API Facts

### INotificationDispatcher
```csharp
Task<NotificationResult> SendAsync(
    string channel,
    string recipient,
    NotificationContext context);
```

### NotificationChannel constants
```csharp
NotificationChannel.Email   // "Email"
NotificationChannel.Sms     // "Sms"
NotificationChannel.Teams   // "Teams"
NotificationChannel.Phone   // "Phone"
```
Always use these constants — never hardcode strings.

### NotificationContext
```csharp
public class NotificationContext
{
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
```

### NotificationResult
```csharp
public class NotificationResult
{
    public int Sent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

---

## Registration (Program.cs)

```csharp
builder.Services.AddSeawayNotifications(options =>
    builder.Configuration.GetSection("SeawayNotifications").Bind(options));
```

**CRITICAL:** Do NOT call `AddSeawayMailer()` separately. `AddSeawayNotifications()` calls it internally. Duplicate registration causes a startup exception.

---

## Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "SeawayNotifications": "Server=SPC-DB01;Database=SeawayNotifications;Integrated Security=True;TrustServerCertificate=True;"
  },
  "SeawayNotifications": {
    "AppKey": "FILE_WATCHER"
  }
}
```

No SMTP settings here. Seaway.Mailer owns all SMTP config in its own `SeawayMailer` section. Seaway.Notifications resolves `IAlertMailer` from DI — the Email channel has no SMTP knowledge.

### Optional channel config (only if channel is used)
```json
"SeawayNotifications": {
  "AppKey": "FILE_WATCHER",
  "Sms":   { "AccountSid": "", "AuthToken": "", "FromNumber": "" },
  "Teams": { "WebhookUrl": "" },
  "Phone": { "AccountSid": "", "AuthToken": "", "FromNumber": "" }
}
```

### AppKey values
| Application | AppKey |
|-------------|--------|
| USPSTracking.FileWatcher | FILE_WATCHER |
| USPSTracking.Web | USPS_TRACKING |
| MailPrep | MAIL_PREP |

---

## Standard Consuming App Pattern

Every Seaway app that uses notifications implements this same 3-table pattern in its own database.

### Entities
```csharp
public class NotificationType
{
    public int Id { get; set; }
    public required string TypeKey { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<NotificationSubscription> Subscriptions { get; set; } = new List<NotificationSubscription>();
}

public class NotificationSubscription
{
    public int Id { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public required string Channel { get; set; }    // use NotificationChannel constants
    public required string Recipient { get; set; }
    public bool IsActive { get; set; } = true;
}

public class NotificationLog
{
    public int Id { get; set; }
    public int NotificationTypeId { get; set; }
    public NotificationType NotificationType { get; set; } = null!;
    public required string Channel { get; set; }
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
```

### Trigger pattern
```csharp
var subscriptions = await _db.NotificationSubscriptions
    .Include(s => s.NotificationType)
    .Where(s => s.NotificationType.TypeKey == "FILE_PROC_FAILED" && s.IsActive)
    .ToListAsync();

foreach (var sub in subscriptions)
{
    var result = await _notifications.SendAsync(
        sub.Channel,
        sub.Recipient,
        new NotificationContext
        {
            Severity = NotificationSeverity.Critical,
            Subject  = "File Processing Failure",
            Body     = $"File {filename} failed at {DateTime.UtcNow:g} UTC."
        });

    _db.NotificationLogs.Add(new NotificationLog
    {
        NotificationTypeId = sub.NotificationTypeId,
        Channel            = sub.Channel,
        Recipient          = sub.Recipient,
        Subject            = "File Processing Failure",
        Success            = result.Sent > 0,
        ErrorMessage       = result.Errors.FirstOrDefault(),
    });
}
await _db.SaveChangesAsync();
```

---

## Channel Implementations

### Email
- Wraps `IAlertMailer` from Seaway.Mailer
- `AlertMailMessage` properties: `To` (required), `Title` (required), `Detail` (required), `Cc`, `Source`, `ExceptionDetail`, `Severity`, `OccurredAt`
- **No `Subject`, `HtmlBody`, or `PlainTextBody`** — those are on `SeawayMailMessage`
- Severity mapping: NotificationSeverity.Critical → AlertSeverity.Critical, Warning → AlertSeverity.Warning, Info → AlertSeverity.Error

### SMS / Phone
- Uses Twilio SDK
- Recipient must be E.164 format: +19205551234
- Blocked until Twilio account is provisioned

### Teams
- Uses Incoming Webhook — HTTP POST with `{ "text": "..." }`
- Recipient is the webhook URL (stored in NotificationSubscriptions.Recipient)
- Blocked until Teams admin creates webhook

---

## Known Issues / Lessons Learned

1. **v1.0.0 packages on feed are wrong** — they implement the old subscription platform model. Always use v1.0.1.
2. **TrustServerCertificate=True required** — SPC-DB01 uses self-signed cert. Without it, connection fails with SSL error.
3. **AddSeawayMailer() must not be called separately** — Notifications calls it internally. Duplicate call causes startup exception.
4. **NotificationChannel constants** — always use these, never hardcode strings. Typos cause silent failures.
5. **No Bind() call = silent failure** — all options stay at defaults, ConnectionString is empty, app throws at startup.

---

## Outstanding Items

| Item | Status |
|------|--------|
| Twilio account provisioned | Blocked — admin action required |
| Teams webhook configured | Blocked — Teams admin required |
| SPC-DB01 app pool permissions for SeawayNotifications | Pending |
| Integrate into USPSTracking.FileWatcher | Pending |
| Integrate into USPSTracking.Web | Pending |
| Integrate into MailPrep | Pending |

---

## Documents

- `Seaway_Notifications_Installation_Guide_v1.0.docx` — database setup, migration, package publishing
- `Seaway_Notifications_Integration_Guide_v1.1.docx` — consuming app pattern, standard entities, SendAsync usage
- `Seaway_Dev_ToDo_v2.docx` — full cross-project todo list
