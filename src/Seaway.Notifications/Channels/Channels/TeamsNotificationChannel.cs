using System.Net.Http.Json;
using Seaway.Notifications.Abstractions;

namespace Seaway.Notifications.Channels;

internal sealed class TeamsNotificationChannel : INotificationChannel
{
    private readonly HttpClient _http;
    private readonly string _webhookUrl;

    public string ChannelName => "Teams";

    public TeamsNotificationChannel(string webhookUrl)
    {
        _http = new HttpClient();
        _webhookUrl = webhookUrl;
    }

    public async Task<bool> SendAsync(string recipient, NotificationContext context)
    {
        try
        {
            var payload = new
            {
                text = $"**{context.Subject}**\n\n{context.Body}"
            };

            var response = await _http.PostAsJsonAsync(_webhookUrl, payload);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
