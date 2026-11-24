using Microsoft.EntityFrameworkCore;
using FinancialSuite.Domain.Repositories;
using FinancialSuite.Infrastructure.Persistence;

namespace FinancialSuite.Infrastructure.Repositories
{
    public class BaseRepository<T>(MyApplicationDbContext dbContext) : IBaseRepository<T> where T : class
    {
        protected MyApplicationDbContext DbContext { get; } = dbContext;

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var data = await DbContext.Set<T>().ToListAsync();
            return data;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            var data = await DbContext.Set<T>().FindAsync(id);
            return data;
        }

        public virtual async Task<Guid> CreateAsync(T entity)
        {
            DbContext.Set<T>().Add(entity);
            await DbContext.SaveChangesAsync();

            // Entities name their key differently (DropDownCategoryId, MediaId, Id), so read it from the EF model.
            var entry = DbContext.Entry(entity);
            var keyName = entry.Metadata.FindPrimaryKey()!.Properties[0].Name;
            return (Guid)entry.Property(keyName).CurrentValue!;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            DbContext.Set<T>().Remove(entity);
            await DbContext.SaveChangesAsync();
        }

        public async Task SaveChanges()
            => await DbContext.SaveChangesAsync();
    }
}
