using System.Collections.Generic;

namespace Backend.DTOs.Listing
{
    public class ListingRequestDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
