using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PaymentType
{
    StudentMembership,
    TutorMembership,
    Lesson,
    WalletTopUp
}
public enum PaymentMethod
{
    PayPal,
    Stripe,
    Card,
    Wallet
}
public enum TransactionStatus
{
    Pending,
    Completed,
    Refunded,
    Failed
}


public class Transaction
{
    [Key]
    public int Id { get; set; }
    public int SenderId { get; set; }
    [ForeignKey(nameof(Transaction.SenderId))]
    public User? Sender { get; set; }
    public int? RecipientId { get; set; } // Nullable if paying the platform
    [ForeignKey(nameof(Transaction.RecipientId))]
    public User? Recipient { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public string? PaymentId { get; set; }
    public DateTime TransactionDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; }
    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }

    public Transaction()
    {

    }
    public override string ToString()
    {
        return $"Transaction: {Id}, SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount:C}, Status: {Status}, PaymentMethod: {PaymentMethod}, Date: {TransactionDate}";
    }
}
