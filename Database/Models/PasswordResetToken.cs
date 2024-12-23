using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    [ForeignKey(nameof(PasswordResetToken.UserId))]
    public User? User { get; set; }
    [Required]
    [MaxLength(255)]
    public string Token { get; set; } = string.Empty;
    [Required]
    public DateTime ExpiryDate { get; set; }
}
