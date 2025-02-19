
using System.Drawing.Printing;
using Microsoft.EntityFrameworkCore;

public class LessonCategoryService : ILessonCategoryService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly ILogger<LessonCategoryService> _logger;

    public LessonCategoryService(
        AvanciraDbContext db,
        ILogger<LessonCategoryService> logger
    )
    {
        _dbContext = db;
        _logger = logger;
    }

    public LessonCategoryDto CreateCategory(LessonCategory category)
    {
        _dbContext.LessonCategories.Add(category);
        _dbContext.SaveChanges();
        return MapToLessonCategoryDto(category);
    }

    public List<LessonCategoryDto> GetLandingPageCategories()
    {
        return _dbContext.LessonCategories
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid())
            .Where(category => category.ShowInDashboard)
            .Take(12)
            .Select(category => MapToLessonCategoryDto(category, _dbContext.Listings.Count(l => l.Active && l.IsVisible && l.LessonCategoryId == category.Id)))
            .ToList();
    }

    public List<LessonCategoryDto> SearchCategories(string? query)
    {
        var queryable = _dbContext.LessonCategories
            .Where(l => EF.Functions.Like(l.Name, $"%{query}%"))
            .AsEnumerable()
            .OrderBy(l => l.Name)
            .Take(10);

        return queryable.Select(category => MapToLessonCategoryDto(category)).ToList();
    }


    public LessonCategoryDto UpdateCategory(int id, LessonCategory updatedCategory)
    {
        var category = _dbContext.LessonCategories.Find(id);
        if (category == null)
            throw new ArgumentException("Can't update Lesson category.");

        category.Name = updatedCategory.Name;
        _dbContext.SaveChanges();
        return MapToLessonCategoryDto(category);
    }

    public bool DeleteCategory(int id)
    {
        var category = _dbContext.LessonCategories.Find(id);
        if (category == null)
        {
            return false; // Or throw an exception
        }

        _dbContext.LessonCategories.Remove(category);
        _dbContext.SaveChanges();
        return true;
    }

    private LessonCategoryDto MapToLessonCategoryDto(LessonCategory category, int? courses = null)
    {
        return new LessonCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = category.ImagePath,
            Courses = courses ?? 0
        };
    }
}
