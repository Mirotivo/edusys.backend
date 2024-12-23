using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        IOptions<SendGridOptions> options,
        ILogger<SendGridEmailService> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var client = new SendGridClient(_options.ApiKey);

        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var to = new EmailAddress(toEmail);
        var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, message, message);

        try
        {
            var response = await client.SendEmailAsync(emailMessage);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Email successfully sent to {Recipient}. Subject: {Subject}, Status Code: {StatusCode}", 
                                        toEmail, subject, response.StatusCode);
            }
            else
            {
                _logger.LogWarning("Email to {Recipient} returned with status code {StatusCode}. Subject: {Subject}", 
                                    toEmail, response.StatusCode, subject);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}. Subject: {Subject}", toEmail, subject);
            throw; // Rethrow for further handling
        }
    }
}
