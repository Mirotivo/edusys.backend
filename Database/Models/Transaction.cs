using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



public class Transaction
{
    [Key]
    public int Id { get; set; }
    public string SenderId { get; set; }
    [ForeignKey(nameof(SenderId))]
    public virtual User Sender { get; set; }
    public string? RecipientId { get; set; } // Nullable if paying the platform
    [ForeignKey(nameof(RecipientId))]
    public virtual User Recipient { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public DateTime TransactionDate { get; set; }
    public TransactionPaymentMethod PaymentMethod { get; set; }
    public TransactionPaymentType PaymentType { get; set; }
    public TransactionStatus Status { get; set; }
    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public string Description { get; internal set; }
    public string? PayPalPaymentId { get; set; } // If PayPal
    public string? StripeCustomerId { get; internal set; } // If Stripe
    public string? StripeCardId { get; internal set; } // If Stripe

    public override string ToString()
    {
        return $"Transaction: {Id}, SenderId: {SenderId}, RecipientId: {RecipientId}, Amount: {Amount:C}, Status: {Status}, Date: {TransactionDate}";
    }
}

