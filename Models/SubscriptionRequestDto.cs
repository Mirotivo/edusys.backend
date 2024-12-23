public enum SubscriptionType
{
    Student,
    Tutor
}
public class SubscriptionRequestDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } // e.g., "PayPal"
    public SubscriptionType SubscriptionType { get; set; }
}
