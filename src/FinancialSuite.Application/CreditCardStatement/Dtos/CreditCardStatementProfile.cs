using AutoMapper;
using FinancialSuite.Application.CreditCardStatement.Commands.Create;
using FinancialSuite.Application.CreditCardStatement.Commands.Update;

namespace FinancialSuite.Application.CreditCardStatement.Dtos
{
    public class CreditCardStatementProfile : Profile
    {
        public CreditCardStatementProfile()
        {
            CreateMap<Domain.Entities.CreditCardStatement, CreditCardStatementDto>()
                .ForMember(dest => dest.CreditCardName, opt => opt.MapFrom(src => src.CreditCard == null ? string.Empty : src.CreditCard.Name))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateCreditCardStatementCommand, Domain.Entities.CreditCardStatement>()
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<UpdateCreditCardStatementCommand, Domain.Entities.CreditCardStatement>()
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
