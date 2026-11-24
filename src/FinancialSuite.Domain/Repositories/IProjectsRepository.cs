using FinancialSuite.Application.Common;
using FinancialSuite.Domain.Entities;

namespace FinancialSuite.Domain.Repositories
{
    public interface IProjectsRepository : IBaseRepository<Project>
    {
        Task<(IEnumerable<Project>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection? sortDirection);
    }
}
