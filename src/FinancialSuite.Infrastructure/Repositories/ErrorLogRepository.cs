using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    internal class ErrorLogRepository(MyApplicationDbContext dbContext) : IErrorLogRepository
    {
        public async Task<Guid> CreateAsync(ErrorLog entity)
        {
            // Discard entities left tracked by the failed request so they are not saved again with the log.
            dbContext.ChangeTracker.Clear();
            dbContext.ErrorLogs.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity.ErrorLogId;
        }
    }
}
