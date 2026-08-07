using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Common;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    /// <summary>
    /// Entity Framework Core implementation of <see cref="IRepository{T}"/>.
    /// <para>
    /// Queries are kept as <see cref="IQueryable{T}"/> for as long as possible so filtering, counting
    /// and paging are executed by the database engine instead of in application memory.
    /// </para>
    /// </summary>
    public class EFRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EFRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual T? GetById(string id) => _dbSet.Find(id);

        public virtual IEnumerable<T> GetAll() => _dbSet.AsNoTracking().ToList();

        /// <summary>
        /// Retained for backwards compatibility with existing callers. Still evaluates client-side,
        /// which is why <see cref="Query"/> exists and should be preferred for large tables.
        /// </summary>
        public virtual IEnumerable<T> Find(Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _dbSet.AsNoTracking().AsEnumerable().Where(predicate).ToList();
        }

        public virtual IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            // The expression tree is translated into SQL, so filtering happens in the database.
            return _dbSet.AsNoTracking().Where(predicate).ToList();
        }

        public virtual PagedResult<T> GetPaged(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _dbSet.AsNoTracking();
            if (predicate is not null)
                query = query.Where(predicate);

            var totalCount = query.Count();

            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
        }

        public virtual void Add(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Add(entity);
        }

        public virtual void AddRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _dbSet.AddRange(entities);
        }

        public virtual void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
        }

        public virtual void Delete(string id)
        {
            var entity = GetById(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }

        public virtual bool Exists(string id) => GetById(id) != null;

        public virtual int Count() => _dbSet.Count();

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _dbSet.Count(predicate);
        }
    }
}
