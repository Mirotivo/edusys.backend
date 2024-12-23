using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum SubscriptionStatus
{
    Active,
    Expired,
    Canceled
}

public class Subscription
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    [ForeignKey(nameof(Subscription.UserId))]
    public User? User { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(50)]
    public string Plan { get; set; }
    public SubscriptionStatus Status { get; set; }

    public Subscription()
    {
        Plan = string.Empty;
    }

    public override string ToString()
    {
        return $"Subscription: {Id}, UserId: {UserId}, Amount: {Amount:C}, StartDate: {StartDate}, EndDate: {EndDate}, Status: {Status}";
    }
}
