using System;
using System.Collections.Generic;

public class PaymentRequestDto
{
    public string Gateway { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AUD";
    public string ReturnUrl { get; set; }
    public string CancelUrl { get; set; }
    public int? ListingId { get; set; }

    public PaymentRequestDto()
    {
        Gateway = string.Empty;
        ReturnUrl = string.Empty;
        CancelUrl = string.Empty;
    }
}

public class CapturePaymentRequestDto
{
    public string Gateway { get; set; }
    public string PaymentId { get; set; }

    public int TransactionId { get; set; }
    public TransactionPaymentMethod PaymentMethod { get; set; }

    public CapturePaymentRequestDto()
    {
        Gateway = string.Empty;
        PaymentId = string.Empty;
    }
}


public class PaymentHistoryDto
{
    public decimal WalletBalance { get; set; }
    public decimal TotalAmountCollected { get; set; }
    public List<TransactionDto> Invoices { get; set; }
    public List<TransactionDto> Transactions { get; set; }

    public PaymentHistoryDto()
    {
        Invoices = new List<TransactionDto>();
        Transactions = new List<TransactionDto>();
    }
}

public class TransactionDto
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string? RecipientId { get; set; }
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

    public TransactionDto()
    {
        SenderId = string.Empty;
        SenderName = string.Empty;
        RecipientName = string.Empty;
        Status = string.Empty;
        TransactionType = string.Empty;
        Description = string.Empty;
        Date = string.Empty;
        Type = string.Empty;
    }
}

public class AddPayPalAccountDto
{
    public string PayPalEmail { get; set; }

    public AddPayPalAccountDto()
    {
        PayPalEmail = string.Empty;
    }
}

public class SaveCardDto
{
    public string StripeToken { get; set; }
    public UserCardType Purpose { get; set; }

    public SaveCardDto()
    {
        StripeToken = string.Empty;
    }
}

public class CardDto
{
    public int Id { get; set; }
    public string Last4 { get; set; }
    public long ExpMonth { get; set; }
    public long ExpYear { get; set; }
    public string Type { get; set; }
    public UserCardType Purpose { get; set; }

    public CardDto()
    {
        Last4 = string.Empty;
        Type = string.Empty;
    }
}

public class PayoutRequest
{
    public string ConnectedAccountId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
public class CreatePayoutRequest
{
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class PayPalAuthRequest
{
    public string AuthCode { get; set; }
}

