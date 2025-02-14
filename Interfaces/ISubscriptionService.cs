using Backend.Database.Models;
using Backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public interface ISubscriptionService
{
    Task<bool> CheckActiveSubscription(string userId);
    Task<(int SubscriptionId, int TransactionId)> CreateSubscription(SubscriptionRequestDto request, string userId);
    Task<List<Subscription>> GetUserSubscriptions(string userId);
    Task<PromoCode> ValidatePromoCode(string promoCode);
    Task<SubscriptionDetailsDto?> GetSubscriptionDetails(string userId);
    Task<bool> ChangeBillingFrequency(string userId, BillingFrequency newFrequency);
    Task<bool> CancelSubscription(string userId);
}
