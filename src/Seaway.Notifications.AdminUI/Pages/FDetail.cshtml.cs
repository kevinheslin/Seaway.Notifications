using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Seaway.Notifications.AdminUI.Services;
using Seaway.Notifications.Data.Entities;

namespace Seaway.Notifications.AdminUI.Pages;

public class DetailModel : PageModel
{
    private readonly NotificationAdminService _service;

    public NotificationType? NotificationType { get; set; }

    [BindProperty]
    public string NewChannel { get; set; } = string.Empty;

    [BindProperty]
    public string NewRecipient { get; set; } = string.Empty;

    public DetailModel(NotificationAdminService service)
    {
        _service = service;
    }

    public async Task OnGetAsync(int id)
    {
        NotificationType = await _service.GetNotificationTypeAsync(id);
    }

    public async Task<IActionResult> OnPostAddAsync(int id)
    {
        if (!string.IsNullOrWhiteSpace(NewChannel) &&
            !string.IsNullOrWhiteSpace(NewRecipient))
        {
            await _service.AddSubscriptionAsync(id, NewChannel, NewRecipient);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, int subscriptionId)
    {
        await _service.ToggleSubscriptionAsync(subscriptionId);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int subscriptionId)
    {
        await _service.RemoveSubscriptionAsync(subscriptionId);
        return RedirectToPage(new { id });
    }
}
