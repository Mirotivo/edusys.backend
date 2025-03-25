public class ListingLessonCategory
{
    public int ListingId { get; set; }
    public int LessonCategoryId { get; set; }

    public virtual Listing Listing { get; set; }
    public virtual LessonCategory LessonCategory { get; set; }
}
