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
    private readonly AvanciraDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AppOptions _appOptions;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<UserService> _logger;


    public UserService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        AvanciraDbContext dbContext,
        INotificationService notificationService,
        IFileUploadService fileUploadService,
        IJwtTokenService jwtTokenService,
        IOptions<AppOptions> appOptions,
        IOptions<JwtOptions> jwtOptions,
        ILogger<UserService> logger

    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _notificationService = notificationService;
        _fileUploadService = fileUploadService;
        _jwtTokenService = jwtTokenService;
        _appOptions = appOptions.Value;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<(bool IsSuccess, string Error)> RegisterUser(RegisterViewModel model, string country)
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
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "An unknown error occurred.";
            return (false, firstError);
        }

        // Assign default role (e.g., "Tutor")
        await _userManager.AddToRoleAsync(user, Role.Tutor.ToString());

        // Handle referral token logic if necessary
        await HandleReferralTokenAsync(model.ReferralToken, user.Id);

        await AssignCountryAsync(user, country);

        return (true, null);
    }

    private async Task HandleReferralTokenAsync(string referralToken, string newUserId)
    {
        if (string.IsNullOrEmpty(referralToken))
        {
            return;
        }

        var referrer = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == referralToken);
        if (referrer == null)
        {
            return;
        }

        _dbContext.Referrals.Add(new Referral
        {
            ReferrerId = referrer.Id,
            ReferredId = newUserId
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

        await _dbContext.SaveChangesAsync();
    }

    private async Task AssignCountryAsync(User user, string country)
    {
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

        var tokenString = await _jwtTokenService.GenerateTokenAsync(user);
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

    public async Task<bool> UpdatePaymentScheduleAsync(string userId, PaymentSchedule paymentSchedule)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            //_logger.LogWarning("User with ID {UserId} not found.", userId);
            return false;
        }

        user.PaymentSchedule = paymentSchedule;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<PaymentSchedule?> GetPaymentScheduleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.PaymentSchedule;
    }



    public async Task<SocialLoginResult> HandleSocialLoginAsync(string provider, string token, string? country = null, string? referralToken = null)
    {
        SocialUser socialUser;

        // Verify token based on the provider
        socialUser = provider.ToLower() switch
        {
            "google" => await VerifyGoogleToken(token),
            "facebook" => await VerifyFacebookToken(token),
            _ => throw new Exception("Invalid provider")
        };

        // Find or create user in the system
        var user = await _userManager.FindByEmailAsync(socialUser.Email);
        if (user == null)
        {
            user = new User
            {
                UserName = socialUser.Email,
                Email = socialUser.Email
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user.");
            }

            // Assign default role (e.g., "Tutor")
            await _userManager.AddToRoleAsync(user, Role.Tutor.ToString());

            // Handle referral token logic if necessary
            await HandleReferralTokenAsync(referralToken, user.Id);

            await AssignCountryAsync(user, country);
        }

        // Generate a JWT token
        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new SocialLoginResult
        {
            Token = tokenResult,
            Roles = roles.ToList()
        };
    }

    private async Task<SocialUser> VerifyGoogleToken(string token)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={token}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GoogleUserPayload>();
        if (payload == null || string.IsNullOrEmpty(payload.Email))
        {
            throw new Exception("Invalid Google token");
        }

        return new SocialUser
        {
            Email = payload.Email,
            Name = payload.Name,
            Picture = payload.Picture,
            Provider = "Google"
        };
    }

    private async Task<SocialUser> VerifyFacebookToken(string token)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,email,name,picture&access_token={token}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<FacebookUserPayload>();
        if (payload == null || string.IsNullOrEmpty(payload.Email))
        {
            throw new Exception("Invalid Facebook token");
        }

        return new SocialUser
        {
            Email = payload.Email,
            Name = payload.Name,
            Picture = payload.Picture?.Data?.Url,
            Provider = "Facebook"
        };
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
