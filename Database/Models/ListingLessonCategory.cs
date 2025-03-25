using System.ComponentModel.DataAnnotations.Schema;

public class ListingLessonCategory
{
    public int ListingId { get; set; }
    public int LessonCategoryId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public virtual Listing Listing { get; set; } = new Listing();

    [ForeignKey(nameof(LessonCategoryId))]
    public virtual LessonCategory LessonCategory { get; set; } = new LessonCategory();
}
