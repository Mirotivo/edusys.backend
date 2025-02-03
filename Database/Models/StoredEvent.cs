namespace Backend.Database.Models
{
    public enum NotificationEvent
    {
        // Authentication-related events
        ConfirmEmail,
        ChangePassword,
        LoginAttempt,

        // Message and chat-related events
        NewMessage,
        ChatRequest,
        MessageRead,

        // Transaction and payment-related events
        PayoutProcessed,
        PaymentFailed,
        PaymentReceived,
        RefundProcessed,

        // Booking or scheduling-related events
        BookingRequested,
        PropositionResponded,
        BookingConfirmed,
        BookingCancelled,
        BookingReminder,

        // Review-related events
        NewReviewReceived,
        NewRecommendationReceived,

        // General notifications
        ProfileUpdated,
        SystemAlert,
        NewFeatureAnnouncement,
    }

    public class StoredEvent
    {
        public int Id { get; set; }
        public NotificationEvent EventType { get; set; }
        public string Data { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Processed { get; set; } = false;
    }
}
