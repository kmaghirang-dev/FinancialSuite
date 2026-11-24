using AutoMapper;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Commands.Create;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSuite.Application.DropDownDetails.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Commands.Update;


namespace FinancialSuite.Application.DropDownDetails.Dtos
{
    public class DropDownDetailsProfile : Profile
    {
        public DropDownDetailsProfile()
        {
            CreateMap<Domain.Entities.DropDownDetail, GetAllDropDownDetailsDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateDropDownDetailsCommand, Domain.Entities.DropDownDetail>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<Domain.Entities.DropDownDetail, GetByIdDropDownDetailsDto>();

            CreateMap<ToggleDropDownDetailsCommand, Domain.Entities.DropDownDetail>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<UpdateDropDownDetailsCommand, Domain.Entities.DropDownDetail>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
