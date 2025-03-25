using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Database.Models;
using Microsoft.AspNetCore.Identity;


public class User : IdentityUser, IAccountable
{
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    [MaxLength(500)]
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    [MaxLength(255)]
    public string? PayPalAccountId { get; set; } // For payouts
    [MaxLength(255)]
    public string? StripeCustomerId { get; set; } // For payments
    [MaxLength(255)]
    public string? StripeConnectedAccountId { get; set; } // For payouts
    [MaxLength(255)]
    public string? SkypeId { get; set; }
    [MaxLength(255)]
    public string? HangoutId { get; set; }
    [MaxLength(255)]
    public string? ProfileImagePath { get; set; }
    [MaxLength(8)]
    public string? RecommendationToken { get; set; }
    public decimal TutorRefundRetention { get; set; } = 50m;
    public UserDiplomaStatus DiplomaStatus { get; set; }
    public UserPaymentSchedule PaymentSchedule { get; set; }

    [MaxLength(255)]
    public string? DiplomaDescription { get; set; }

    [MaxLength(255)]
    public string? DiplomaPath { get; set; }
    public int? CountryId { get; set; }
    [ForeignKey(nameof(User.CountryId))]
    public virtual Country? Country { get; set; }
    public List<UserCard> UserCards { get; set; }
    public Address Address { get; set; }

    [MaxLength(50)]
    public string? TimeZoneId { get; set; } = "Australia/Sydney";

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }


    [NotMapped]
    public string FullName
    {
        get
        {
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return $"{FirstName} {LastName}";
            }
            else if (!string.IsNullOrEmpty(FirstName))
            {
                return FirstName;
            }
            else if (!string.IsNullOrEmpty(LastName))
            {
                return LastName;
            }
            else
            {
                return Email ?? string.Empty;
            }
        }
    }

    [NotMapped]
    public string PaymentGateway
    {
        get
        {
            if (!string.IsNullOrEmpty(PayPalAccountId))
            {
                return "PayPal";
            }
            else
            {
                return "Stripe";
            }
        }
    }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        UserCards = new List<UserCard>();
        Address = new Address();
    }

    public override string ToString()
    {
        return $"User: {Id}, Name: {FirstName} {LastName}, Email: {Email}, Active: {Active}, CreatedAt: {CreatedAt}";
    }
}

