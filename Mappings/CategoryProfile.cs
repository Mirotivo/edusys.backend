using AutoMapper;
using Backend.DTOs.Category;

namespace Backend.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<LessonCategory, CategoryDto>()
                .ReverseMap();
        }
    }

}
