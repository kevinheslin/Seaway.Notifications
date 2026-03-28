using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Seaway.Notifications.AdminUI.Services;

namespace Seaway.Notifications.AdminUI;

public static class NotificationsAdminExtensions
{
    public static IServiceCollection AddSeawayNotificationsAdminUI(
        this IServiceCollection services)
    {
        services.AddScoped<NotificationAdminService>();
        services.AddRazorPages();
        return services;
    }

    public static IEndpointRouteBuilder MapSeawayNotificationsAdminUI(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRazorPages();
        return endpoints;
    }
}

