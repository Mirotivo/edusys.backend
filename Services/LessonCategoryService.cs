
public class LessonCategoryService : ILessonCategoryService
{
    private readonly skillseekDbContext _db;

    public LessonCategoryService(skillseekDbContext db)
    {
        _db = db;
    }


    public List<LessonCategory> GetDashboardCategories()
    {
        return _db.LessonCategories
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid()) // Randomize the order
            .Take(10) // Limit to 10 random listings (optional)
            .ToList();
    }

    public List<LessonCategory> GetCategories()
    {
        return _db.LessonCategories.ToList();
    }

    public LessonCategory CreateCategory(LessonCategory category)
    {
        _db.LessonCategories.Add(category);
        _db.SaveChanges();
        return category;
    }

    public LessonCategory UpdateCategory(int id, LessonCategory updatedCategory)
    {
        var category = _db.LessonCategories.Find(id);
        if (category == null)
        {
            return null; // Or throw an exception
        }

        category.Name = updatedCategory.Name;
        _db.SaveChanges();
        return category;
    }

    public bool DeleteCategory(int id)
    {
        var category = _db.LessonCategories.Find(id);
        if (category == null)
        {
            return false; // Or throw an exception
        }

        _db.LessonCategories.Remove(category);
        _db.SaveChanges();
        return true;
    }
}
