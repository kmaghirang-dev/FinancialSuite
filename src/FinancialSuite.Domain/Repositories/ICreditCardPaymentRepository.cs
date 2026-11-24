using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Domain.Repositories
{
    public interface ICreditCardPaymentRepository : IBaseRepository<CreditCardPayment>
    {
        Task<(IEnumerable<CreditCardPayment>, int)> GetAllMatchingAsync(Guid? creditCardId, DateOnly? from, DateOnly? to, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
    }
}
