using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Database.Models
{
    public class PromoCode
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        public decimal DiscountAmount { get; set; } // Fixed discount amount

        public double DiscountPercentage { get; set; } // Percentage discount

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

}

