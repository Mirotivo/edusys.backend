using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class AddressDto
{
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FormattedAddress { get; set; }
}

public class UserDto
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImagePath { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public AddressDto? Address { get; set; }
    public string? TimeZoneId { get; set; } = "Austarlia/Sydney";
    public string? SkypeId { get; set; }
    public string? HangoutId { get; set; }
    public string? RecommendationToken { get; set; }
    public List<string>? ProfileVerified { get; set; }
    public bool? IsStripeConnected { get; set; }
    public bool? IsPayPalConnected { get; set; }

    // Analytics
    public int? LessonsCompleted { get; set; }
    public int? Evaluations { get; set; }
    public int ProfileCompletion { get; set; }
}
public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
public class CompensationUpdateDto
{
    public int Percentage { get; set; }
}
public class PaymentScheduleDto
{
    [Required]
    public UserPaymentSchedule PaymentSchedule { get; set; }
}

