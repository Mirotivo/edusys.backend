using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



public class Transaction
{
    [Key]
    public int Id { get; set; }
    public string SenderId { get; set; }
    [ForeignKey(nameof(Transaction.SenderId))]
    public User? Sender { get; set; }
    public string? RecipientId { get; set; } // Nullable if paying the platform
    [ForeignKey(nameof(Transaction.RecipientId))]
    public User? Recipient { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public string? PaymentId { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionPaymentType PaymentType { get; set; }
    public TransactionPaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; }
    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }

    public Transaction()
    {
        SenderId = string.Empty;
    }
    public override string ToString()
    {
        return $"Transaction: {Id}, SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount:C}, Status: {Status}, PaymentMethod: {PaymentMethod}, Date: {TransactionDate}";
    }
}
