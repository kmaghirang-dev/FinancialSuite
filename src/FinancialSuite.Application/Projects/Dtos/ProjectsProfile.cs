using AutoMapper;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Projects.Commands.Toggle;
using FinancialSuite.Application.Projects.Commands.Update;

namespace FinancialSuite.Application.Projects.Dtos
{
    public class ProjectsProfile : Profile
    {
        public ProjectsProfile()
        {
            CreateMap<Domain.Entities.Project, GetAllProjectsDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<Domain.Entities.Project, GetByIdProjectDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateProjectCommand, Domain.Entities.Project>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateProjectCommand, Domain.Entities.Project>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<ToggleProjectCommand, Domain.Entities.Project>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
