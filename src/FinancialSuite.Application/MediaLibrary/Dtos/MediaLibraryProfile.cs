using AutoMapper;
using FinancialSuite.Application.MediaLibrary.Commands.Create;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;
using FinancialSuite.Application.MediaLibrary.Commands.Update;

namespace FinancialSuite.Application.MediaLibrary.Dtos
{
    public class MediaLibraryProfile : Profile
    {
        public MediaLibraryProfile()
        {
            CreateMap<Domain.Entities.MediaLibrary, GetAllMediaLibraryDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<Domain.Entities.MediaLibrary, GetByIdMediaLibraryDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateMediaLibraryCommand, Domain.Entities.MediaLibrary>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateMediaLibraryCommand, Domain.Entities.MediaLibrary>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<ToggleMediaLibraryCommand, Domain.Entities.MediaLibrary>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
