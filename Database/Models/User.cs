using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Database.Models;
using Microsoft.AspNetCore.Identity;

[Flags]
public enum Role
{
    None = 0,
    Student = 1, // 01 in binary
    Tutor = 2    // 10 in binary
}
public enum DiplomaStatus
{
    NotSubmitted = 0,
    UnderReview = 1,
    Verified = 2
}

public enum PaymentSchedule
{
    Monthly = 0,
    PerLesson = 1
}

public class User : IdentityUser, IAccountable
{
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    [MaxLength(500)]
    public string? AboutMe { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(255)]
    public string? Address { get; set; }
    [MaxLength(255)]
    public string? PayPalAccountId { get; set; } // For payouts
    [MaxLength(255)]
    public string? StripeCustomerId { get; set; } // For payouts
    [MaxLength(255)]
    public string? SkypeId { get; set; }
    [MaxLength(255)]
    public string? HangoutId { get; set; }
    [MaxLength(255)]
    public string? ProfileImagePath { get; set; }
    [MaxLength(8)]
    public string? RecommendationToken { get; set; }
    public decimal TutorRefundRetention { get; set; } = 50m;
    public DiplomaStatus DiplomaStatus { get; set; }
    public PaymentSchedule PaymentSchedule { get; set; }

    [MaxLength(255)]
    public string? DiplomaDescription { get; set; }

    [MaxLength(255)]
    public string? DiplomaPath { get; set; }
    public int? CountryId { get; set; }
    [ForeignKey(nameof(User.CountryId))]
    public virtual Country? Country { get; set; }
    public List<UserCard> UserCards { get; set; }

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        UserCards = new List<UserCard>();
    }

    public override string ToString()
    {
        return $"User: {Id}, Name: {FirstName} {LastName}, Email: {Email}, Active: {Active}, CreatedAt: {CreatedAt}";
    }
}
