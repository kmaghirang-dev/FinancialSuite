using AutoMapper;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownCategory.Commands.Update;

namespace FinancialSuite.Application.DropDownCategory.Dtos
{
    public class DropDownCategoryProfile : Profile
    {
        public DropDownCategoryProfile()
        {
            CreateMap<Domain.Entities.DropDownCategory, GetAllDropDownCategoryDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<Domain.Entities.DropDownCategory, GetByIdDropDownCategoryDto>()
                .ForMember(dest => dest.DropDownDetails, opt => opt.MapFrom(src => src.DropDownDetails));

            CreateMap<CreateDropDownCategoryCommand, Domain.Entities.DropDownCategory>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateDropDownCategoryCommand, Domain.Entities.DropDownCategory>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<ToggleDropDownCategoryCommand, Domain.Entities.DropDownCategory>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
