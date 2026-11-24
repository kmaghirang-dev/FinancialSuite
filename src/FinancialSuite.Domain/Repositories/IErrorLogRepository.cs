using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Domain.Repositories
{
    public interface IErrorLogRepository
    {
        Task<Guid> CreateAsync(ErrorLog entity);
    }
}
