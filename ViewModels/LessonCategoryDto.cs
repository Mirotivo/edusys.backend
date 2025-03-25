public class LessonCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Image { get; set; }
    public int Courses { get; set; }

    public LessonCategoryDto()
    {
        Name = string.Empty;
    }
}

