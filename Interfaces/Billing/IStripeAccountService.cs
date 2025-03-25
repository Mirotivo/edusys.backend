using System.Threading.Tasks;

namespace Backend.Interfaces.Billing
{
    public interface IStripeAccountService
    {
        // Create
        Task<string> ConnectStripeAccountAsync(string userId);
    }
}

