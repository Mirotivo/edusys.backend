using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Referral : ICreatable
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReferrerId { get; set; }
    [ForeignKey(nameof(Referral.ReferrerId))]
    public virtual User? Referrer { get; set; }

    [Required]
    public int ReferredId { get; set; }
    [ForeignKey(nameof(Referral.ReferredId))]
    public virtual User? Referred { get; set; }

    public DateTime CreatedAt { get; set; }
}
