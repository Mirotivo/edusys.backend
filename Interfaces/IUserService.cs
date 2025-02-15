public interface IUserService
{
    Task<(bool IsSuccess, string Error)> RegisterUser(RegisterViewModel model, string country);
    Task<SocialLoginResult> SocialLoginAsync(string provider, string token, string? country = null, string? referralToken = null);
    Task<(bool IsSuccess, string Error)> ConfirmEmailAsync(string userId, string token);
    Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model);
    Task<UserDto?> GetUser(string userId);
    Task<UserDto?> GetUserByToken(string recommendationToken);
    Task<bool> UpdateUser(string userId, UserDto updatedUser);
    Task<bool> DeleteAccountAsync(string userId);
    Task<bool> SendPasswordResetEmail(string email);
    Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
    Task<bool> ResetPassword(string email, string token, string newPassword);
    Task<DiplomaStatus> GetDiplomaStatusAsync(string userId);
    Task<bool> SubmitDiplomaAsync(string userId, IFormFile diplomaFile, string? diplomaDescription = null);
    Task<decimal> GetCompensationPercentageAsync(string userId);
    Task<bool> UpdatePaymentScheduleAsync(string userId, PaymentSchedule paymentSchedule);
    Task<PaymentSchedule?> GetPaymentScheduleAsync(string userId);
    Task UpdateCompensationPercentageAsync(string userId, int newPercentage);
}
