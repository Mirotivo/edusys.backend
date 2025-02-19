using Backend.Database.Models;
using Backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public interface ISubscriptionService
{
    // Create
    Task<(int SubscriptionId, int TransactionId)> CreateSubscriptionAsync(SubscriptionRequestDto request, string userId);

    // Read
    Task<bool> HasActiveSubscriptionAsync(string userId);
    Task<List<Subscription>> ListUserSubscriptionsAsync(string userId);
    Task<PromoCode> ValidatePromoCode(string promoCode);
    Task<SubscriptionDetailsDto?> FetchSubscriptionDetailsAsync(string userId);

    // Update
    Task<bool> ChangeBillingFrequencyAsync(string userId, BillingFrequency newFrequency);

    // Delete
    Task<bool> CancelSubscriptionAsync(string userId);
}
