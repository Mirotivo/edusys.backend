public interface IUserService
{
    Task<bool> RegisterUser(RegisterViewModel model);
    Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model);
    Task<UserDto?> GetUser(int userId);
    Task<UserDto?> GetUserByToken(string recommendationToken);
    Task<bool> UpdateUser(int userId, UserDto updatedUser);
    Task<bool> DeleteAccountAsync(int userId);
    Task<bool> SendPasswordResetEmail(string email);
    Task<bool> ResetPassword(string token, string newPassword);
    Task<DiplomaStatus> GetDiplomaStatusAsync(int userId);
    Task<bool> SubmitDiplomaAsync(int userId, IFormFile diplomaFile);
    Task<decimal> GetCompensationPercentageAsync(int userId);
    Task UpdateCompensationPercentageAsync(int userId, int newPercentage);
}
