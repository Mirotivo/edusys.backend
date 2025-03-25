using System.Collections.Generic;
using System;
using Backend.DTOs.Category;

namespace Backend.DTOs.Listing
{
    public class ListingResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
