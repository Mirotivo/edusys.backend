public class Notification
{
    public NotificationEvent EventName { get; set; } // E.g., "NewUserRegistered", "OrderPlaced"
    public string Message { get; set; }  // Human-readable message
    public object Data { get; set; }     // Optional: Additional data (e.g., user info, order details)
}
