public class Notification
{
    public NotificationEvent EventName { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }

    public Notification()
    {
        Message = string.Empty;
    }
}

