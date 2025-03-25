using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Database.Models;
using Backend.Services.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AvanciraDbContext _dbContext;
    private readonly ISubscriptionService _subscriptionService;
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
        ISubscriptionService subscriptionService,
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
        _subscriptionService = subscriptionService;
        _notificationService = notificationService;
        _fileUploadService = fileUploadService;
        _jwtTokenService = jwtTokenService;
        _appOptions = appOptions.Value;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<(bool IsSuccess, string Error)> RegisterUserAsync(RegisterViewModel model, string country)
    {
        var user = new User
        {
            UserName = model.Email,
            Email = model.Email,
            TimeZoneId = model.TimeZoneId
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            // Handle registration errors (e.g., duplicate email, weak password)
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "An unknown error occurred.";
            return (false, firstError);
        }

        // Assign default role (e.g., "Tutor")
        await _userManager.AddToRoleAsync(user, UserRole.Tutor.ToString());

        // Handle referral token logic if necessary
        await HandleReferralTokenAsync(model.ReferralToken, user.Id);

        await HandleAssignCountryAsync(user, country);

        // Generate confirmation email token
        await HandleSendEmailConfirmationAsync(user);


        return (true, null);
    }

    public async Task<SocialLoginResult> SocialLoginAsync(string provider, string token, string? country = null, string? referralToken = null)
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
        var isRegistered = true;
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
            await _userManager.AddToRoleAsync(user, UserRole.Tutor.ToString());

            // Handle referral token logic if necessary
            await HandleReferralTokenAsync(referralToken, user.Id);

            await HandleAssignCountryAsync(user, country);

            // Generate confirmation email token
            await HandleSendEmailConfirmationAsync(user);

            isRegistered = false;
        }

        // Generate a JWT token
        var tokenResult = await _jwtTokenService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new SocialLoginResult
        {
            Token = tokenResult,
            Roles = roles.ToList(),
            isRegistered = isRegistered
        };
    }

    public async Task<(bool IsSuccess, string Error)> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token.Replace(" ", "+"));
        if (result.Succeeded)
        {
            return (true, null);
        }
        else
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Email confirmation failed.";
            return (false, firstError);
        }
    }


    public async Task<(string Token, List<string> Roles)?> LoginUserAsync(LoginViewModel model)
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


    public async Task<string?> GetPaymentGatewayAsync(string userId)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.PayPalAccountId, u.StripeConnectedAccountId })
            .FirstOrDefaultAsync();

        if (user == null) return null;

        if (!string.IsNullOrEmpty(user.PayPalAccountId))
            return "PayPal";

        if (!string.IsNullOrEmpty(user.StripeConnectedAccountId))
            return "Stripe";

        return null;
    }

    public async Task<UserDto?> GetUserAsync(string userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Address)
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

    public async Task<UserDto?> GetUserByReferralTokenAsync(string recommendationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == recommendationToken);
        if (user == null) return null;

        return user == null ? null : MapToUserDto(user);
    }

    public async Task<decimal> GetCompensationPercentageAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        return user.TutorRefundRetention;
    }

    public async Task<UserDiplomaStatus> GetDiplomaStatusAsync(string userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return user.DiplomaStatus; // Return "notSubmitted", "underReview", or "verified"
    }

    public async Task<UserPaymentSchedule?> GetPaymentScheduleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.PaymentSchedule;
    }

    public async Task<List<object>> GetLandingPageUsersAsync()
    {
        var targetCities = new List<string> { "Sydney", "Brisbane", "Perth" };

        // Fetch actual mentor counts from the database
        var jobLocations = await _dbContext.Users
            .Where(u => u.Address != null && targetCities.Contains(u.Address.City))
            .GroupBy(u => u.Address.City)
            .Select(group => new
            {
                City = group.Key,
                Mentors = group.Count()
            })
            .ToDictionaryAsync(x => x.City, x => x.Mentors);

        // Define default job locations and override counts if data exists
        var predefinedLocations = targetCities.Select(city => (object)new
        {
            Img = $"assets/img/city/city_{city.ToLower()}.jpg",
            City = city,
            Country = "Australia",
            Mentors = jobLocations.GetValueOrDefault(city, 0)
        }).ToList();

        return predefinedLocations;
    }

    public async Task<bool> ModifyUserAsync(string userId, UserDto updatedUser)
    {
        var user = await _dbContext.Users
                                   .Include(u => u.Address)
                                   .AsTracking()
                                   .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.FirstName = updatedUser.FirstName ?? user.FirstName;
        user.LastName = updatedUser.LastName ?? user.LastName;
        user.Bio = updatedUser.Bio ?? user.Bio;
        user.DateOfBirth = updatedUser.DateOfBirth ?? user.DateOfBirth;
        user.DateOfBirth = user.DateOfBirth.HasValue ? DateTime.SpecifyKind(user.DateOfBirth.Value, DateTimeKind.Utc) : user.DateOfBirth;
        user.PhoneNumber = updatedUser.PhoneNumber ?? user.PhoneNumber;
        user.SkypeId = updatedUser.SkypeId ?? user.SkypeId;
        user.HangoutId = updatedUser.HangoutId ?? user.HangoutId;
        user.TimeZoneId = updatedUser.TimeZoneId ?? user.TimeZoneId;
        // Handle profile image replacement
        user.ProfileImagePath = await _fileUploadService.ReplaceFileAsync(updatedUser.ProfileImage, user.ProfileImagePath, "profiles");
        // Update Address
        if (updatedUser.Address != null)
        {
            var userAddress = user.Address;
            if (userAddress == null)
            {
                userAddress = new Backend.Database.Models.Address();
                user.Address = userAddress;
            }

            userAddress.StreetAddress = updatedUser.Address.StreetAddress ?? string.Empty;
            userAddress.City = updatedUser.Address.City ?? string.Empty;
            userAddress.State = updatedUser.Address.State ?? string.Empty;
            userAddress.Country = updatedUser.Address.Country ?? string.Empty;
            userAddress.PostalCode = updatedUser.Address.PostalCode ?? string.Empty;
            userAddress.Latitude = updatedUser.Address.Latitude ?? 0.0d;
            userAddress.Longitude = updatedUser.Address.Longitude ?? 0.0d;
            userAddress.FormattedAddress = updatedUser.Address.FormattedAddress ?? string.Empty;
        }

        _dbContext.Users.Update(user);
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
        var eventData = new ChangePasswordEvent
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            ResetLink = resetLink,
            ExpiryDate = DateTime.UtcNow.AddHours(1)
        };

        await _notificationService.NotifyAsync(NotificationEvent.ChangePassword, eventData);
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

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ModifyPaymentScheduleAsync(string userId, UserPaymentSchedule paymentSchedule)
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
    public async Task UpdateCompensationPercentageAsync(string userId, int newPercentage)
    {
        if (newPercentage < 0 || newPercentage > 100)
            throw new ArgumentException("Compensation percentage must be between 0% and 100%.");

        var user = await _dbContext.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.TutorRefundRetention = newPercentage;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> SubmitDiplomaAsync(string userId, IFormFile diplomaFile, string? diplomaDescription = null)
    {
        var user = await _dbContext.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Upload the diploma file (pseudo-code)
        var diplomaPath = await _fileUploadService.SaveFileAsync(diplomaFile, "diplomas");

        user.DiplomaDescription = diplomaDescription;
        user.DiplomaPath = diplomaPath;
        user.DiplomaStatus = UserDiplomaStatus.UnderReview;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }




    public async Task<bool> DeleteAccountAsync(string userId)
    {
        var user = await _dbContext.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false; // User not found
        }

        // Mark user as inactive
        user.Active = false;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();

        return true;
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


        // Call the existing CreateSubscription method
        var request = new SubscriptionRequestDto
        {
            BillingFrequency = SubscriptionBillingFrequency.Monthly.ToString(),
            Amount = 0, // Free trial month
            //PaymentMethodId = null, // No payment method for free month
            PromoCode = null // No promo code used
        };

        await _subscriptionService.CreateSubscriptionAsync(request, referrer.Id);
    }
    private async Task HandleAssignCountryAsync(User user, string country)
    {
        // Assign the country to the user
        var countryEntity = await _dbContext.Countries.FirstOrDefaultAsync(c => EF.Functions.Like(c.Code, country));
        if (countryEntity == null)
        {
            countryEntity = new Country { Name = country, Code = country };
            _dbContext.Countries.Add(countryEntity);
            await _dbContext.SaveChangesAsync();
        }
        user.CountryId = countryEntity.Id;
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
    private async Task HandleSendEmailConfirmationAsync(User user)
    {
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_appOptions.FrontEndUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}&userId={user.Id}";

        var eventData = new ConfirmEmailEvent
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            ConfirmationLink = confirmationLink
        };

        await _notificationService.NotifyAsync(NotificationEvent.ConfirmEmail, eventData);
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


    private int CalculateProfileCompletion(User user)
    {
        var totalFields = 8;
        var completedFields = 0;

        if (!string.IsNullOrEmpty(user.FirstName)) completedFields++;
        if (!string.IsNullOrEmpty(user.LastName)) completedFields++;
        if (!string.IsNullOrEmpty(user.Bio)) completedFields++;
        if (!string.IsNullOrEmpty(user.Email)) completedFields++;
        if (user.EmailConfirmed) completedFields++;
        if (user.DateOfBirth.HasValue) completedFields++;
        if (!string.IsNullOrEmpty(user.PhoneNumber)) completedFields++;
        //if (!string.IsNullOrEmpty(user.Address)) completedFields++;
        if (!string.IsNullOrEmpty(user.ProfileImagePath)) completedFields++;

        return (int)((completedFields / (double)totalFields) * 100);
    }


    private UserDto MapToUserDto(User user, int? lessonsCompleted = null, int? evaluations = null)
    {
        var profileVerified = new List<string>();
        if (user.EmailConfirmed)
        {
            profileVerified.Add("Email");
        }
        if (user.PhoneNumberConfirmed)
        {
            profileVerified.Add("Phone");
        }
        AddressDto addressDto = null;
        var address = user.Address;
        if (address != null)
        {
            addressDto = new AddressDto
            {
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                FormattedAddress = address.FormattedAddress
            };
        }

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Bio = user.Bio,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
            Address = addressDto,
            TimeZoneId = user.TimeZoneId,
            ProfileImagePath = user.ProfileImagePath,
            SkypeId = user.SkypeId,
            HangoutId = user.HangoutId,
            ProfileVerified = profileVerified,
            RecommendationToken = user.RecommendationToken,
            LessonsCompleted = lessonsCompleted ?? 0,
            Evaluations = evaluations ?? 0,
            IsStripeConnected = !string.IsNullOrEmpty(user.StripeConnectedAccountId),
            IsPayPalConnected = !string.IsNullOrEmpty(user.PayPalAccountId),
            ProfileCompletion = CalculateProfileCompletion(user)
        };
    }
}

