using AutoMapper;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.CreditCardPayment.Commands.Create;
using FinancialSuite.Application.CreditCardPayment.Commands.Update;

namespace FinancialSuite.Application.CreditCardPayment.Dtos
{
    public class CreditCardPaymentProfile : Profile
    {
        public CreditCardPaymentProfile()
        {
            CreateMap<Domain.Entities.CreditCardPayment, CreditCardPaymentDto>()
                .ForMember(dest => dest.CreditCardName, opt => opt.MapFrom(src => src.CreditCard == null ? string.Empty : CreditCardOverviewCalculator.DisplayName(src.CreditCard)))
                .ForMember(dest => dest.StatementId, opt => opt.Ignore())
                .ForMember(dest => dest.StatementPeriodEndDate, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated.ToString("MM-dd-yyyy")))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.DateUpdated.ToString("MM-dd-yyyy")));

            CreateMap<CreateCreditCardPaymentCommand, Domain.Entities.CreditCardPayment>()
                .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => (src.PaymentReference ?? string.Empty).Trim()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));

            CreateMap<UpdateCreditCardPaymentCommand, Domain.Entities.CreditCardPayment>()
                .ForMember(dest => dest.PaymentReference, opt => opt.MapFrom(src => (src.PaymentReference ?? string.Empty).Trim()))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => (src.Notes ?? string.Empty).Trim()))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
