using System.Threading.Tasks;

namespace Backend.Interfaces.Billing
{
    public interface IPayPalAccountService
    {
        // Create
        Task<bool> ConnectPayPalAccountAsync(string userId, string authCode);
    }
}

