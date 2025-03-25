using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<ListingDto> CreateListingAsync(CreateListingDto createListingDto, string userId)
    {
        // Handle image upload
        var imageUrl = await _fileUploadService.ReplaceFileAsync(createListingDto.ListingImage, createListingDto.ListingImagePath, "listings");

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
                    if (Enum.TryParse<ListingLocationType>(location, true, out var parsedLocation))
                    {
                        return parsedLocation;
                    }
                    else
                    {
                        return ListingLocationType.None;
                    }
                })
                .Aggregate(ListingLocationType.None, (current, parsedLocation) => current | parsedLocation)
                ?? ListingLocationType.None,
            ListingLessonCategories = new List<ListingLessonCategory>
            {
                new ListingLessonCategory { LessonCategoryId = lessonCategoryId }
            },
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
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.Rates)
            .FirstOrDefaultAsync(l => l.Id == listing.Id);

        if (loadedListing == null)
        {
            throw new InvalidOperationException("Failed to reload the listing after saving.");
        }

        return MapToListingDto(loadedListing);
    }


    public ListingDto GetListingById(int id)
    {
        var listing = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User).ThenInclude(l => l.Address)
            .FirstOrDefault(l => l.Id == id);

        return listing == null ? null : MapToListingDto(listing);
    }

    public async Task<PagedResult<ListingDto>> GetUserListingsAsync(string userId, int page, int pageSize)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.UserId == userId);

        // Get total count before pagination
        var totalResults = await queryable.CountAsync();

        // Apply pagination
        var lessons = await queryable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = lessons.Select(listing => MapToListingDto(listing)).ToList();

        return new PagedResult<ListingDto>
        {
            TotalResults = totalResults,
            Page = page,
            PageSize = pageSize,
            Results = results
        };
    }

    public IEnumerable<ListingDto> GetLandingPageListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.IsVisible)
            .OrderBy(_ => Guid.NewGuid())
            .Take(50)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public IEnumerable<ListingDto> GetLandingPageTrendingListings()
    {
        var listings = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User)
            .Where(l => l.Active && l.IsVisible)
            .OrderBy(_ => Guid.NewGuid())
            .Take(10)
            .ToList();

        return listings.Select(l => MapToListingDto(l));
    }

    public PagedResult<ListingDto> SearchListings(string query, List<string> categories, int page, int pageSize, double? lat = null, double? lng = null, double radiusKm = 10)
    {
        var queryable = _dbContext.Listings
            .Include(l => l.Rates)
            .Include(l => l.ListingLessonCategories).ThenInclude(l => l.LessonCategory)
            .Include(l => l.User).ThenInclude(l => l.Address)
            .Where(l => EF.Functions.Like(l.Title, $"%{query}%") ||
                        EF.Functions.Like(l.Description, $"%{query}%") ||
                        EF.Functions.Like(l.AboutLesson, $"%{query}%") ||
                        EF.Functions.Like(l.AboutYou, $"%{query}%"));
        // Apply category filtering if categories are selected
        if (categories.Any())
        {
            queryable = queryable.Where(l => l.ListingLessonCategories.Any(j => categories.Contains(j.LessonCategory.Name)));
        }

        if (lat.HasValue && lng.HasValue)
        {
            // Haversine formula to calculate distance in KM
            queryable = queryable.Where(l =>
                l.User.Address.Latitude != 0 && l.User.Address.Longitude != 0 &&
                (6371 * Math.Acos(
                    Math.Cos(Math.PI * lat.Value / 180) * Math.Cos(Math.PI * l.User.Address.Latitude / 180) *
                    Math.Cos(Math.PI * (lng.Value - l.User.Address.Longitude) / 180) +
                    Math.Sin(Math.PI * lat.Value / 180) * Math.Sin(Math.PI * l.User.Address.Latitude / 180)
                )) <= radiusKm);
        }

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

    public ListingStatisticsDto GetListingStatistics()
    {
        var today = DateTime.UtcNow.Date;
        var startOfDay = today;
        var endOfDay = today.AddDays(1);

        var stats = _dbContext.Listings
            .AsNoTracking()
            .Where(l => l.Active && l.IsVisible)
            .GroupBy(_ => 1)
            .Select(g => new ListingStatisticsDto
            {
                TotalListings = g.Count(),
                NewListingsToday = g.Count(l => startOfDay <= l.CreatedAt && l.CreatedAt < endOfDay)
            })
            .FirstOrDefault() ?? new ListingStatisticsDto { TotalListings = 0, NewListingsToday = 0 };

        return stats;
    }

    public async Task<bool> ModifyListingTitleAsync(int listingId, string userId, string newTitle)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.Title = newTitle;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingImageAsync(int listingId, string userId, IFormFile newImage)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        var imageUrl = await _fileUploadService.ReplaceFileAsync(newImage, listing.ListingImagePath, "listings");
        listing.ListingImagePath = imageUrl;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingLocationsAsync(int listingId, string userId, List<string> newLocations)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        // Convert list of location strings to LocationType enum
        var updatedLocations = newLocations
            .Select(location =>
            {
                if (Enum.TryParse<ListingLocationType>(location, true, out var parsedLocation))
                {
                    return parsedLocation;
                }
                return ListingLocationType.None;
            })
            .Aggregate(ListingLocationType.None, (current, parsedLocation) => current | parsedLocation);

        listing.Locations = updatedLocations;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingDescriptionAsync(int listingId, string userId, string newAboutLesson, string newAboutYou)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.AboutLesson = newAboutLesson;
        listing.AboutYou = newAboutYou;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingCategoryAsync(int listingId, string userId, int newCategoryId)
    {
        var listing = await _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        var categoryExists = await _dbContext.LessonCategories.AnyAsync(c => c.Id == newCategoryId);
        if (!categoryExists) return false;

        listing.ListingLessonCategories = new List<ListingLessonCategory>
        {
            new ListingLessonCategory { LessonCategoryId = newCategoryId }
        };
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ModifyListingRatesAsync(int listingId, string userId, RatesDto newRates)
    {
        var listing = await _dbContext.Listings
            .Include(l => l.Rates)
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (listing == null) return false;

        listing.Rates.Hourly = newRates.Hourly;
        listing.Rates.FiveHours = newRates.FiveHours;
        listing.Rates.TenHours = newRates.TenHours;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleListingVisibilityAsync(int id)
    {
        var listing = _dbContext.Listings.AsTracking().FirstOrDefault(l => l.Id == id);
        if (listing != null)
        {
            listing.IsVisible = !listing.IsVisible;
            _dbContext.Listings.Update(listing);
            await _dbContext.SaveChangesAsync();
        }
        return true;
    }


    public async Task<bool> DeleteListingAsync(int listingId, string userId)
    {
        var listing = _dbContext.Listings
            .Where(l => l.Id == listingId && l.UserId == userId)
            .AsTracking()
            .FirstOrDefault();

        if (listing == null || !listing.Active)
        {
            return false;
        }

        listing.Active = false;
        _dbContext.Listings.Update(listing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private ListingDto MapToListingDto(Listing listing, int? contactedCount = null)
    {
        AddressDto addressDto = null;
        var address = listing.User?.Address;
        if (address != null)
        {
            addressDto = new AddressDto
            {
                StreetAddress = address.StreetAddress,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                FormattedAddress = address.FormattedAddress
            };
        }

        return new ListingDto
        {
            Id = listing.Id,
            IsVisible = listing.IsVisible,
            TutorId = listing.User.Id,
            TutorName = listing.User?.FullName,
            TutorBio = listing.User?.Bio,
            ContactedCount = contactedCount ?? 0,
            TutorAddress = addressDto,
            Reviews = 0,
            LessonCategory = listing.ListingLessonCategories.FirstOrDefault()?.LessonCategory.Name,
            Title = listing.Title,
            ListingImagePath = listing.ListingImagePath,
            Locations = Enum.GetValues(typeof(ListingLocationType))
                            .Cast<ListingLocationType>()
                            .Where(location => (listing.Locations & location) == location && location != ListingLocationType.None)
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

