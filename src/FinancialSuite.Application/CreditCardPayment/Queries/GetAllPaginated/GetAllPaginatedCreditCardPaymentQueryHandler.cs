using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.CreditCardPayment.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardPayment.Queries.GetAllPaginated
{
    public class GetAllPaginatedCreditCardPaymentQueryHandler(ILogger<GetAllPaginatedCreditCardPaymentQueryHandler> logger, IMapper mapper, ICreditCardPaymentRepository repository, ICreditCardRepository cardRepository) : IRequestHandler<GetAllPaginatedCreditCardPaymentQuery, PagedResult<CreditCardPaymentDto>>
    {
        public async Task<PagedResult<CreditCardPaymentDto>> Handle(GetAllPaginatedCreditCardPaymentQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await repository.GetAllMatchingAsync(
                request.CreditCardId,
                request.From,
                request.To,
                request.SearchPhrase,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var payments = data.ToList();
            var cards = (await cardRepository.GetWithHistoryByIdsAsync(payments.Select(x => x.CreditCardId)))
                .ToDictionary(x => x.CreditCardId);

            var resultDto = payments.Select(payment =>
            {
                var dto = mapper.Map<CreditCardPaymentDto>(payment);
                if (cards.TryGetValue(payment.CreditCardId, out var card))
                {
                    var statement = CreditCardDebtCalculator.FindStatementFor(payment.PaymentDate, card.InitialBalanceDate, card.Statements.Select(CreditCardDebtCalculator.ToLedger));
                    dto.StatementId = statement?.StatementId;
                    dto.StatementPeriodEndDate = statement?.PeriodEndDate;
                    dto.IsBeforeInitialBalance = payment.PaymentDate <= card.InitialBalanceDate;
                }
                return dto;
            }).ToList();

            return new PagedResult<CreditCardPaymentDto>(resultDto, totalCount, request.PageSize, request.PageNumber);
        }
    }
}
