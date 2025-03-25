using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

public class TwilioSmsService : ISmsService
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(
        IOptions<TwilioOptions> options,
        ILogger<TwilioSmsService> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        try
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_options.FromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );

            _logger.LogInformation("SMS sent to {PhoneNumber}. SID: {MessageSid}", toPhoneNumber, messageResource.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toPhoneNumber);
            throw; // Rethrow exception if needed
        }
    }
}

