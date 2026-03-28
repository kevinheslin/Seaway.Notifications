using Seaway.Notifications.Abstractions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using static System.Net.WebRequestMethods;

namespace Seaway.Notifications.Channels;

internal sealed class PhoneNotificationChannel : INotificationChannel
{
    private readonly string _fromNumber;

    public string ChannelName => "Phone";

    public PhoneNotificationChannel(string accountSid, string authToken, string fromNumber)
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
            var call = await CallResource.CreateAsync(
                to: new Twilio.Types.PhoneNumber(recipient),
                from: new Twilio.Types.PhoneNumber(_fromNumber),
                twiml: new Twilio.Types.Twiml(
                    $"<Response><Say>{context.Subject}. {context.Body}</Say></Response>"));

            return (true, $"SID: {call.Sid} Status: {call.Status}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
