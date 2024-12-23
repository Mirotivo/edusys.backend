public interface ISmsService
{
    Task SendSmsAsync(string toPhoneNumber, string message);
}
