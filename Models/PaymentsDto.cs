public class PaymentRequestDto
{
    public string Gateway { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string ReturnUrl { get; set; }
    public string CancelUrl { get; set; }
    public int? ListingId { get; set; }
}

public class CapturePaymentRequestDto
{
    public string Gateway { get; set; }
    public string PaymentId { get; set; }
}


public class PaymentHistoryDto
{
    public decimal WalletBalance { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public List<TransactionDto> Invoices { get; set; }
    public List<TransactionDto> Transactions { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public int? RecipientId { get; set; }
    public string RecipientName { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal Net { get; set; }
    public string Status { get; set; }
    public string TransactionType { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Date { get; set; }
    public string Type { get; set; }
}

public class AddPayPalAccountDto
{
    public string PayPalEmail { get; set; }
}

public class SaveCardDto
{
    public string StripeToken { get; set; }
    public CardType Purpose { get; set; }
}

public class CardDto
{
    public int Id { get; set; }
    public string Last4 { get; set; }
    public long ExpMonth { get; set; }
    public long ExpYear { get; set; }
    public string Type { get; set; }
    public CardType Purpose { get; set; }
}