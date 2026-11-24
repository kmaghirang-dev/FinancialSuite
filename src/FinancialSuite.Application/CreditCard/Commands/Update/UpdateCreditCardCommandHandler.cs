using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Commands.Update
{
    public class UpdateCreditCardCommandHandler(
        ILogger<UpdateCreditCardCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardRepository repository) : IRequestHandler<UpdateCreditCardCommand>
    {
        public async Task Handle(UpdateCreditCardCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.CreditCardId;
            var data = await repository.GetWithHistoryAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), id.ToString());

            // The initial balance is the baseline: every statement has to come after it. Payments may be on any date;
            // those on or before the baseline are already in it and are not counted.
            var firstStatement = data.Statements.OrderBy(x => x.PeriodEndDate).FirstOrDefault();
            if (firstStatement != null && firstStatement.PeriodEndDate <= request.InitialBalanceDate)
                throw new ConflictException($"The initial balance date must be before the first statement's period end ({firstStatement.PeriodEndDate:MMM d, yyyy}).");

            mapper.Map(request, data);
            data.UpdatedBy = currentUser.UserName;

            // A new initial balance changes the first statement's opening balance.
            CreditCardDebtCalculator.SyncOpeningBalances(data.InitialBalance, data.Statements);

            await repository.SaveChanges();
        }
    }
}
