using Seaway.Notifications.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Seaway.Notifications.Channels;

internal sealed class SmsNotificationChannel : INotificationChannel
{
    private readonly string _fromNumber;

    public string ChannelName => "Sms";

    public SmsNotificationChannel(string accountSid, string authToken, string fromNumber)
    {
        TwilioClient.Init(accountSid, authToken);
        _fromNumber = fromNumber;
    }

    public async Task<(bool Success, string Response)> SendAsync(
        string recipient,
        NotificationContext context)
    {
        try
        {
            var message = await MessageResource.CreateAsync(
                to: new Twilio.Types.PhoneNumber(recipient),
                from: new Twilio.Types.PhoneNumber(_fromNumber),
                body: $"{context.Subject}: {context.Body}");

            return (true, $"SID: {message.Sid} Status: {message.Status}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}