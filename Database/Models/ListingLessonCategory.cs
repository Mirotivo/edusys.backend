public class ListingLessonCategory
{
    public int ListingId { get; set; }
    public Listing Listing { get; set; }

    public int LessonCategoryId { get; set; }
    public LessonCategory LessonCategory { get; set; }
}
