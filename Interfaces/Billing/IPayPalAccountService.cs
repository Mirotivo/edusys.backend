using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using Stripe;

namespace Backend.Interfaces.Billing
{
    public interface IPayPalAccountService
    {
        // Create
        Task<bool> ConnectPayPalAccountAsync(string userId, string authCode);
    }
}
