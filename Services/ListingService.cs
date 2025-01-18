using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public class ListingService : IListingService
{
    private readonly AvanciraDbContext _dbContext;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ListingService> _logger;

    public ListingService(
        AvanciraDbContext dbContext,
        IFileUploadService fileUploadService,
        ILogger<ListingService> logger
    )
    {
        _dbContext = dbContext;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    public IEnumerable<ListingDto> GetDashboardListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .AsEnumerable()
            .OrderBy(_ => Guid.NewGuid())
            .Take(10)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public PagedResult<ListingDto> SearchListings(string query, int page, int pageSize)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => EF.Functions.Like(l.Title, $"%{query}%") ||
                        EF.Functions.Like(l.Description, $"%{query}%") ||
                        EF.Functions.Like(l.AboutLesson, $"%{query}%") ||
                        EF.Functions.Like(l.AboutYou, $"%{query}%"));

        var totalResults = queryable.Count();
        var listings = queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var results = listings.Select(l => MapToListingDto(l));

        return new PagedResult<ListingDto>
        {
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize,
            Results = results
        };
    }

    public IEnumerable<ListingDto> GetUserListings(string userId)
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.UserId == userId)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public ListingDto GetListingById(int id)
    {
        var listing = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.LessonCategory)
            .Include(l => l.User)
            .FirstOrDefault(l => l.Id == id);

        return listing == null ? null : MapToListingDto(listing);
    }

    public async Task<ListingDto> CreateListing(CreateListingDto createListingDto, string userId)
    {
        // Handle image upload
        var imageUrl = await _fileUploadService.UpdateFileAsync(createListingDto.ListingImage, createListingDto.ListingImagePath, "listings");

        int lessonCategoryId;

        // Check for an existing category
        if (createListingDto.LessonCategoryId.HasValue)
        {
            var existingCategory = await _dbContext.LessonCategories
                .FirstOrDefaultAsync(c => c.Id == createListingDto.LessonCategoryId.Value);

            if (existingCategory == null)
            {
                throw new ArgumentException($"LessonCategoryId {createListingDto.LessonCategoryId.Value} is invalid. The category does not exist.");
            }

            lessonCategoryId = existingCategory.Id;
        }
        else if (!string.IsNullOrWhiteSpace(createListingDto.LessonCategory))
        {
            // Handle new category
            var existingCategoryByName = await _dbContext.LessonCategories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == createListingDto.LessonCategory!.ToLower());

            if (existingCategoryByName != null)
            {
                // Category already exists
                lessonCategoryId = existingCategoryByName.Id;
            }
            else
            {
                // Create a new category
                var newCategory = new LessonCategory
                {
                    Name = createListingDto.LessonCategory
                };

                _dbContext.LessonCategories.Add(newCategory);
                await _dbContext.SaveChangesAsync();

                lessonCategoryId = newCategory.Id;
            }
        }
        else
        {
            throw new ArgumentException("Either LessonCategoryId or LessonCategory must be provided.");
        }

        // Create listing
        var listing = new Listing
        {
            Title = createListingDto.Title,
            AboutLesson = createListingDto.AboutLesson,
            AboutYou = createListingDto.AboutYou,
            ListingImagePath = imageUrl,
            Locations = createListingDto.Locations?
                .Select(location =>
                {
                    if (Enum.TryParse<LocationType>(location, true, out var parsedLocation))
                    {
                        return parsedLocation;
                    }
                    else
                    {
                        return LocationType.None;
                    }
                })
                .Aggregate(LocationType.None, (current, parsedLocation) => current | parsedLocation)
                ?? LocationType.None,
            LessonCategoryId = lessonCategoryId,
            UserId = userId,
            Rates = new ListingRates
            {
                Hourly = createListingDto.Rates.Hourly,
                FiveHours = createListingDto.Rates.FiveHours,
                TenHours = createListingDto.Rates.TenHours
            }
        };

        await _dbContext.Listings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        // Reload the listing with related data
        var loadedListing = await _dbContext.Listings
            .Include(l => l.User)
            .Include(l => l.LessonCategory)
            .Include(l => l.Rates)
            .FirstOrDefaultAsync(l => l.Id == listing.Id);

        if (loadedListing == null)
        {
            throw new InvalidOperationException("Failed to reload the listing after saving.");
        }

        return MapToListingDto(loadedListing);
    }

    private ListingDto MapToListingDto(Listing listing, int? contactedCount = null)
    {
        return new ListingDto
        {
            Id = listing.Id,
            TutorId = listing.User.Id,
            TutorName = listing.User?.FirstName,
            ContactedCount = contactedCount ?? 0,
            Reviews = 0,
            LessonCategory = listing.LessonCategory?.Name,
            Title = listing.Title,
            ListingImagePath = listing.ListingImagePath,
            Locations = Enum.GetValues(typeof(LocationType))
                            .Cast<LocationType>()
                            .Where(location => (listing.Locations & location) == location && location != LocationType.None)
                            .Select(location => location.ToString())
                            .ToList(),
            AboutLesson = listing.AboutLesson,
            AboutYou = listing.AboutYou,
            Rate = $"{listing.Rates.Hourly}/h",
            Rates = new RatesDto
            {
                Hourly = listing.Rates.Hourly,
                FiveHours = listing.Rates.FiveHours,
                TenHours = listing.Rates.TenHours
            },
            SocialPlatforms = new List<string> { "Messenger", "Linkedin", "Facebook", "Email" }
        };
    }
}
