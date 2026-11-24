using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class CreditCardPaymentRepository(MyApplicationDbContext dbContext)
        : BaseRepository<CreditCardPayment>(dbContext), ICreditCardPaymentRepository
    {
        public override async Task<CreditCardPayment?> GetByIdAsync(Guid id)
            => await DbContext.CreditCardPayments
                .Include(x => x.CreditCard)
                .FirstOrDefaultAsync(x => x.CreditCardPaymentId == id);

        public async Task<(IEnumerable<CreditCardPayment>, int)> GetAllMatchingAsync(Guid? creditCardId, DateOnly? from, DateOnly? to, string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = DbContext.CreditCardPayments
                .Include(x => x.CreditCard)
                .Where(x =>
                    (creditCardId == null || x.CreditCardId == creditCardId) &&
                    (from == null || x.PaymentDate >= from) &&
                    (to == null || x.PaymentDate <= to) &&
                    (searchPhraseLower == null ||
                     x.PaymentReference.ToLower().Contains(searchPhraseLower) ||
                     x.Notes.ToLower().Contains(searchPhraseLower) ||
                     x.CreditCard!.Name.ToLower().Contains(searchPhraseLower) ||
                     x.CreditCard!.Nickname.ToLower().Contains(searchPhraseLower))
                )
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.DateCreated);

            var totalCount = await baseQuery.CountAsync();

            if (sortBy != null)
            {
                var columnSelector = new Dictionary<string, Expression<Func<CreditCardPayment, object>>>
                {
                    { nameof(CreditCardPayment.PaymentDate), r => r.PaymentDate },
                    { nameof(CreditCardPayment.Amount), r => r.Amount },
                    { nameof(CreditCardPayment.DateCreated), r => r.DateCreated }
                };

                var selectedColumn = columnSelector[sortBy];

                baseQuery = (sortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn))
                    .ThenByDescending(x => x.DateCreated);
            }

            var data = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
