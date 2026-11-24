using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Delete
{
    public class DeleteCreditCardPaymentCommandHandler(
        ILogger<DeleteCreditCardPaymentCommandHandler> logger,
        ICreditCardPaymentRepository repository) : IRequestHandler<DeleteCreditCardPaymentCommand>
    {
        public async Task Handle(DeleteCreditCardPaymentCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardPayment), id.ToString());

            await repository.DeleteAsync(data);
        }
    }
}
