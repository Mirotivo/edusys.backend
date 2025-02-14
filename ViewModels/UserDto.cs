using System.ComponentModel.DataAnnotations;

public class UserDto
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AboutMe { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImagePath { get; set; }
    public IFormFile? ProfileImage { get; set; }
    public string? Address { get; set; }
    public string? SkypeId { get; set; }
    public string? HangoutId { get; set; }
    public string? RecommendationToken { get; set; }
    public List<string>? ProfileVerified { get; set; }
    public bool? PaymentDetailsAvailable { get; set; }

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
    public PaymentSchedule PaymentSchedule { get; set; }
}
