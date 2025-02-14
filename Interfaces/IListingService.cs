public interface IListingService
{
    IEnumerable<ListingDto> GetLandingPageListings();
    PagedResult<ListingDto> SearchListings(string query, int page, int pageSize);
    IEnumerable<ListingDto> GetUserListings(string userId);
    ListingDto GetListingById(int id);
    Task<ListingDto> CreateListing(CreateListingDto createListingDto, string userId);
}
