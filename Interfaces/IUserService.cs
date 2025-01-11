public interface IUserService
{
    Task<(bool IsSuccess, string Error)> RegisterUser(RegisterViewModel model, string country);
    Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model);
    Task<UserDto?> GetUser(string userId);
    Task<UserDto?> GetUserByToken(string recommendationToken);
    Task<bool> UpdateUser(string userId, UserDto updatedUser);
    Task<bool> DeleteAccountAsync(string userId);
    Task<bool> SendPasswordResetEmail(string email);
    Task<bool> ResetPassword(string email, string token, string newPassword);
    Task<DiplomaStatus> GetDiplomaStatusAsync(string userId);
    Task<bool> SubmitDiplomaAsync(string userId, IFormFile diplomaFile);
    Task<decimal> GetCompensationPercentageAsync(string userId);
    Task<bool> UpdatePaymentScheduleAsync(string userId, PaymentSchedule paymentSchedule);
    Task<PaymentSchedule?> GetPaymentScheduleAsync(string userId);
    Task UpdateCompensationPercentageAsync(string userId, int newPercentage);
}
