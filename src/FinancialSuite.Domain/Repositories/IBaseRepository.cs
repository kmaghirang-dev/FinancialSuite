namespace FinancialSuite.Domain.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChanges();
    }
}
