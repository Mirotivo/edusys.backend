using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ListingRates
{
    [Key]
    public int Id { get; set; }

    public int ListingId { get; set; }
    [ForeignKey(nameof(ListingRates.ListingId))]
    public Listing? Listing { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Hourly rate must be a positive value.")]
    public decimal Hourly { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Five hours rate must be a positive value.")]
    public decimal FiveHours { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Ten hours rate must be a positive value.")]
    public decimal TenHours { get; set; }
}
