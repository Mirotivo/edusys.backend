using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class SubscriptionRequestDto
{
    public string? PayPalPaymentId { get; set; }
    public decimal? Amount { get; set; }
    public TransactionPaymentMethod PaymentMethod { get; set; }
    public TransactionPaymentType PaymentType { get; set; }
    public string BillingFrequency { get; set; }
    public string? PromoCode { get; set; }

    public SubscriptionRequestDto()
    {
        BillingFrequency = string.Empty;
    }
}
public class ChangeFrequencyRequestDto
{
    [Required]
    public SubscriptionBillingFrequency NewFrequency { get; set; }
}

public class SubscriptionDetailsDto
{
    public string Plan { get { return BillingFrequency; } }
    public string BillingFrequency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime NextBillingDate { get; set; }
    public decimal NextBillingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public List<SubscriptionHistoryDto> SubscriptionHistory { get; set; } = new();
}
public class SubscriptionHistoryDto
{
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ChangeDate { get; set; }
    public string BillingFrequency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

