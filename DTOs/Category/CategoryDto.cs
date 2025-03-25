namespace Backend.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool DisplayInLandingPage { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

}
