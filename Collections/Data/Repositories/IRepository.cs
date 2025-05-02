using System.Linq.Expressions;

namespace Collections.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // --- Read ---
        Task<TEntity> GetByIdAsync(object id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        // --- Create ---
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        // --- Update ---
        void Update(TEntity entity);
        // --- Delete ---
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
