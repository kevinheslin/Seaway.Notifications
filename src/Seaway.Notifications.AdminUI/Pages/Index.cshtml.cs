using Microsoft.AspNetCore.Mvc.RazorPages;
using Seaway.Notifications.AdminUI.Services;
using Seaway.Notifications.Data.Entities;

namespace Seaway.Notifications.AdminUI.Pages;

public class IndexModel : PageModel
{
    private readonly NotificationAdminService _service;
    private readonly string _appKey;

    public List<NotificationType> NotificationTypes { get; set; } = new();

    public IndexModel(NotificationAdminService service, string appKey)
    {
        _service = service;
        _appKey = appKey;
    }

    public async Task OnGetAsync()
    {
        NotificationTypes = await _service.GetNotificationTypesAsync(_appKey);
    }
}
