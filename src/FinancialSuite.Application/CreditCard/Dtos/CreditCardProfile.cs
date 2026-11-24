using AutoMapper;
using FinancialSuite.Application.CreditCard.Commands.Create;
using FinancialSuite.Application.CreditCard.Commands.Toggle;
using FinancialSuite.Application.CreditCard.Commands.Update;

namespace FinancialSuite.Application.CreditCard.Dtos
{
    public class CreditCardProfile : Profile
    {
        public CreditCardProfile()
        {
            CreateMap<Domain.Entities.CreditCard, CreditCardDto>()
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateCreditCardCommand, Domain.Entities.CreditCard>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => (src.Nickname ?? string.Empty).Trim()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateCreditCardCommand, Domain.Entities.CreditCard>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => (src.Nickname ?? string.Empty).Trim()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<ToggleCreditCardCommand, Domain.Entities.CreditCard>()
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
