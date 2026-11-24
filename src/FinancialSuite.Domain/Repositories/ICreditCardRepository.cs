using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Domain.Repositories
{
    public interface ICreditCardRepository : IBaseRepository<CreditCard>
    {
        Task<(IEnumerable<CreditCard>, int)> GetAllMatchingAsync(bool? isActive, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
        // Cards with their statements and payments loaded, which the debt calculator needs.
        Task<CreditCard?> GetWithHistoryAsync(Guid id);
        Task<IEnumerable<CreditCard>> GetAllWithHistoryAsync(bool? isActive);
        Task<IEnumerable<CreditCard>> GetWithHistoryByIdsAsync(IEnumerable<Guid> ids);
    }
}
