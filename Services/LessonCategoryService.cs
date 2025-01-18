
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;

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


    public List<LessonCategory> GetDashboardCategories()
    {
        return _dbContext.LessonCategories
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid()) // Randomize the order
            .Take(12) // Limit to 10 random listings (optional)
            .ToList();
    }

    public List<LessonCategory> GetCategories(string? query)
    {
        var queryable = _dbContext.LessonCategories
            .Where(l => EF.Functions.Like(l.Name, $"%{query}%"))
            .AsEnumerable()
            .OrderBy(l => l.Name)
            .Take(10);

        return queryable.ToList();
    }

    public LessonCategory CreateCategory(LessonCategory category)
    {
        _dbContext.LessonCategories.Add(category);
        _dbContext.SaveChanges();
        return category;
    }

    public LessonCategory UpdateCategory(int id, LessonCategory updatedCategory)
    {
        var category = _dbContext.LessonCategories.Find(id);
        if (category == null)
            throw new ArgumentException("Can't update Lesson category.");

        category.Name = updatedCategory.Name;
        _dbContext.SaveChanges();
        return category;
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
}
