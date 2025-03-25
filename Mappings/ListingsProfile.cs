using AutoMapper;
using Backend.DTOs.Listing;
using System.Linq;

namespace Backend.Mappings
{
    public class ListingsProfile : Profile
    {
        public ListingsProfile()
        {
            CreateMap<ListingRequestDto, Listing>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            CreateMap<Listing, ListingResponseDto>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.ListingLessonCategories.Select(lc => lc.LessonCategory)));
        }
    }
}
