using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly skillseekDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IFileUploadService _fileUploadService;
    private readonly AppOptions _appOptions;
    private readonly JwtOptions _jwtOptions;


    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        skillseekDbContext dbContext,
        INotificationService notificationService,
        IFileUploadService fileUploadService,
        IOptions<AppOptions> appOptions,
        IOptions<JwtOptions> jwtOptions

    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _notificationService = notificationService;
        _fileUploadService = fileUploadService;
        _appOptions = appOptions.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<bool> RegisterUser(RegisterViewModel model, string country)
    {
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            // Handle registration errors (e.g., duplicate email, weak password)
            return false;
        }

        // Assign default role (e.g., "Tutor")
        await _userManager.AddToRoleAsync(user, Role.Tutor.ToString());

        // Handle referral token logic if necessary
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

        // Assign the country to the user
        var countryEntity = await _dbContext.Countries.FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, country));
        if (countryEntity == null)
        {
            countryEntity = new Country { Name = country };
            _dbContext.Countries.Add(countryEntity);
            await _dbContext.SaveChangesAsync();
        }
        user.CountryId = countryEntity.Id;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return null; // Invalid login attempt
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FirstName ?? user.Email ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key ?? string.Empty)), SecurityAlgorithms.HmacSha256Signature),
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, roles.ToList());
    }

    public async Task<UserDto?> GetUser(string userId)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (user == null) return null;

        var lessonsCompleted = await _dbContext.Lessons
            .Include(l => l.Listing)
            .CountAsync(l => l.Listing != null && l.Listing.UserId == userId && l.Status == LessonStatus.Completed);

        var evaluations = await _dbContext.Reviews
            .CountAsync(r => r.RevieweeId == userId && (r.Type == ReviewType.Review || r.Type == ReviewType.Recommendation));

        return MapToUserDto(user, lessonsCompleted, evaluations);
    }

    public async Task<UserDto?> GetUserByToken(string recommendationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == recommendationToken);
        if (user == null) return null;

        return user == null ? null : MapToUserDto(user);
    }

    public async Task<bool> UpdateUser(string userId, UserDto updatedUser)
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

    public async Task<bool> DeleteAccountAsync(string userId)
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
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false; // User not found
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_appOptions.FrontEndUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

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

    public async Task<bool> ResetPassword(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<DiplomaStatus> GetDiplomaStatusAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return user.DiplomaStatus; // Return "notSubmitted", "underReview", or "verified"
    }

    public async Task<bool> SubmitDiplomaAsync(string userId, IFormFile diplomaFile)
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


    public async Task<decimal> GetCompensationPercentageAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        return user.TutorRefundRetention;
    }

    public async Task UpdateCompensationPercentageAsync(string userId, int newPercentage)
    {
        if (newPercentage < 0 || newPercentage > 100)
            throw new ArgumentException("Compensation percentage must be between 0% and 100%.");

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.TutorRefundRetention = newPercentage;

        await _dbContext.SaveChangesAsync();
    }

    private UserDto MapToUserDto(User user, int? lessonsCompleted = null, int? evaluations = null, bool? paymentDetailsAvailable = null)
    {
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
            ProfileVerified = new List<string>(),
            RecommendationToken = user.RecommendationToken,
            LessonsCompleted = lessonsCompleted ?? 0,
            Evaluations = evaluations ?? 0,
            PaymentDetailsAvailable = paymentDetailsAvailable ?? !string.IsNullOrEmpty(user.StripeCustomerId)
        };
    }
}
