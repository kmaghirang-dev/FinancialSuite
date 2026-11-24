using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class CreditCardRepository(MyApplicationDbContext dbContext)
        : BaseRepository<CreditCard>(dbContext), ICreditCardRepository
    {
        public async Task<(IEnumerable<CreditCard>, int)> GetAllMatchingAsync(bool? isActive, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = DbContext.CreditCards.Where(x =>
                    (isActive == null || x.IsActive == isActive) &&
                    (searchPhraseLower == null || x.Name.ToLower().Contains(searchPhraseLower) || x.Nickname.ToLower().Contains(searchPhraseLower))
                )
                .OrderBy(x => x.Name)
                .ThenBy(x => x.CreditCardId);

            var totalCount = await baseQuery.CountAsync();

            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<CreditCard, object>>>
                {
                    { nameof(CreditCard.Name), r => r.Name },
                    { nameof(CreditCard.InitialBalanceDate), r => r.InitialBalanceDate },
                    { nameof(CreditCard.DateCreated), r => r.DateCreated }
                };

                var selectedColumn = columnSelector[sortBy];

                baseQuery = (sortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn))
                    .ThenBy(x => x.CreditCardId);
            }

            // Statements and payments are loaded so the list can show each card's current outstanding balance.
            var data = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.Statements)
                .Include(x => x.Payments)
                .AsSplitQuery()
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<CreditCard?> GetWithHistoryAsync(Guid id)
            => await WithHistory().FirstOrDefaultAsync(x => x.CreditCardId == id);

        public async Task<IEnumerable<CreditCard>> GetAllWithHistoryAsync(bool? isActive)
            => await WithHistory()
                .Where(x => isActive == null || x.IsActive == isActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

        public async Task<IEnumerable<CreditCard>> GetWithHistoryByIdsAsync(IEnumerable<Guid> ids)
        {
            var idList = ids.Distinct().ToList();
            return await WithHistory().Where(x => idList.Contains(x.CreditCardId)).ToListAsync();
        }

        private IQueryable<CreditCard> WithHistory()
            => DbContext.CreditCards
                .Include(x => x.Statements)
                .Include(x => x.Payments)
                .AsSplitQuery();
    }
}
