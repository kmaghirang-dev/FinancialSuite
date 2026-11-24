using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class CreditCardStatementRepository(MyApplicationDbContext dbContext)
        : BaseRepository<CreditCardStatement>(dbContext), ICreditCardStatementRepository
    {
    }
}
