using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class UserService : IUserService
{
    private readonly skillseekDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AppOptions _appOptions;
    private readonly JwtOptions _jwtOptions;


    public UserService(
        skillseekDbContext dbContext,
        INotificationService notificationService,
        IFileUploadService fileUploadService,
        IPasswordHasher<User> passwordHasher,
        IOptions<AppOptions> appOptions,
        IOptions<JwtOptions> jwtOptions

    )
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _fileUploadService = fileUploadService;
        _passwordHasher = passwordHasher;
        _appOptions = appOptions.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<bool> RegisterUser(RegisterViewModel model)
    {
        if (await _dbContext.Users.AnyAsync(u => EF.Functions.Like(u.Email, model.Email)))
        {
            return false; // User already exists
        }

        var user = new User
        {
            Email = model.Email,
            Roles = Role.Tutor
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        if (!string.IsNullOrEmpty(model.ReferralToken))
        {
            var referrer = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == model.ReferralToken);
            if (referrer != null)
            {
                _dbContext.Referrals.Add(new Referral
                {
                    ReferrerId = referrer.Id,
                    ReferredId = user.Id
                });

                _dbContext.Subscriptions.Add(new Subscription
                {
                    UserId = referrer.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Amount = 0,
                    Status = SubscriptionStatus.Active,
                    Plan = "Premium"
                });
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Active && EF.Functions.Like(u.Email, model.Email));
        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
        {
            return null; // Invalid credentials
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key ?? "")),
                SecurityAlgorithms.HmacSha256Signature)
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var roles = Enum.GetValues(typeof(Role))
            .Cast<Role>()
            .Where(r => r != Role.None && user.Roles.HasFlag(r))
            .Select(r => r.ToString())
            .ToList();

        return (tokenString, roles);
    }

    public async Task<UserDto?> GetUser(int userId)
    {
        return await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                Phone = u.Phone,
                Address = u.Address,
                ProfileImagePath = u.ProfileImagePath,
                SkypeId = u.SkypeId,
                HangoutId = u.HangoutId,
                ProfileVerified = new List<string>(),
                RecommendationToken = u.RecommendationToken,
                LessonsCompleted = _dbContext.Lessons.Include(l => l.Listing).Count(l => l.Listing.UserId == userId && l.Status == LessonStatus.Completed),
                Evaluations = _dbContext.Reviews.Count(r => r.RevieweeId == userId && (r.Type == ReviewType.Review || r.Type == ReviewType.Recommendation)),
                PaymentDetailsAvailable = !string.IsNullOrEmpty(u.StripeCustomerId)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetUserByToken(string recommendationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == recommendationToken);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            Phone = user.Phone,
            Address = user.Address,
            ProfileImagePath = user.ProfileImagePath,
            SkypeId = user.SkypeId,
            HangoutId = user.HangoutId,
            ProfileVerified = new List<string>()
        };
    }

    public async Task<bool> UpdateUser(int userId, UserDto updatedUser)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.FirstName = updatedUser.FirstName ?? user.FirstName;
        user.LastName = updatedUser.LastName ?? user.LastName;
        user.DateOfBirth = updatedUser.DateOfBirth ?? user.DateOfBirth;
        user.Phone = updatedUser.Phone ?? user.Phone;
        user.Address = updatedUser.Address ?? user.Address;
        user.SkypeId = updatedUser.SkypeId ?? user.SkypeId;
        user.HangoutId = updatedUser.HangoutId ?? user.HangoutId;

        // Handle profile image replacement
        user.ProfileImagePath = await _fileUploadService.UpdateFileAsync(updatedUser.ProfileImage, user.ProfileImagePath, "profiles");

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAccountAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false; // User not found
        }

        // Mark user as inactive
        user.Active = false;
        await _dbContext.SaveChangesAsync();

        return true;
    }


    public async Task<bool> SendPasswordResetEmail(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.Like(u.Email, email));
        if (user == null)
        {
            return false; // User not found
        }

        // Generate a reset token
        var token = Guid.NewGuid().ToString();
        var resetLink = $"{_appOptions.FrontEndUrl}/reset-password?token={token}";

        // Save the token in the database
        _dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        });
        await _dbContext.SaveChangesAsync();

        // Send the reset email
        var emailBody = $"<p>Hi {user.FirstName},</p>" +
                        $"<p>Click <a href=\"{resetLink}\">here</a> to reset your password. The link will expire in 1 hour.</p>";

        await _notificationService.NotifyAsync(
            user.Id.ToString(),
            NotificationEvent.ChangePassword,
            "Password Reset Request",
            new
            {
                EmailSubject = "Password Reset Request",
                EmailBody = emailBody,
                ResetLink = resetLink,
                ExpiryDate = DateTime.UtcNow.AddHours(1)
            }
        );

        return true;
    }

    public async Task<bool> ResetPassword(string token, string newPassword)
    {
        // Validate the token
        var tokenEntry = await _dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.ExpiryDate > DateTime.UtcNow);

        if (tokenEntry == null)
        {
            return false; // Invalid or expired token
        }

        // Get the user
        var user = await _dbContext.Users.FindAsync(tokenEntry.UserId);
        if (user == null)
        {
            return false; // User not found
        }

        // Update the password
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        _dbContext.PasswordResetTokens.Remove(tokenEntry); // Invalidate the token
        await _dbContext.SaveChangesAsync();

        return true;
    }


    public async Task<DiplomaStatus> GetDiplomaStatusAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return user.DiplomaStatus; // Return "notSubmitted", "underReview", or "verified"
    }

    public async Task<bool> SubmitDiplomaAsync(int userId, IFormFile diplomaFile)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Upload the diploma file (pseudo-code)
        var diplomaPath = await _fileUploadService.SaveFileAsync(diplomaFile, "diplomas");

        user.DiplomaPath = diplomaPath;
        user.DiplomaStatus = DiplomaStatus.UnderReview;

        await _dbContext.SaveChangesAsync();
        return true;
    }


    public async Task<decimal> GetCompensationPercentageAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        return user.TutorRefundRetention;
    }

    public async Task UpdateCompensationPercentageAsync(int userId, int newPercentage)
    {
        if (newPercentage < 0 || newPercentage > 100)
            throw new ArgumentException("Compensation percentage must be between 0% and 100%.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.TutorRefundRetention = newPercentage;

        await _dbContext.SaveChangesAsync();
    }

}
