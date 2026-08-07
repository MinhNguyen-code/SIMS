using System.Linq.Expressions;
using DayNeCu3726.Common;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of <see cref="IRepository{T}"/>, used for tests and demos.
    /// <para>
    /// Liskov Substitution Principle: it honours exactly the same contract as
    /// <see cref="EF.EFRepository{T}"/>, so a service under test behaves identically whichever
    /// implementation is injected.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Entity type exposing a string identifier.</typeparam>
    public abstract class InMemoryRepository<T> : IRepository<T> where T : class
    {
        protected static readonly List<T> _store = new();
        protected abstract string GetId(T entity);

        public T? GetById(string id) => _store.FirstOrDefault(e => GetId(e) == id);

        public IEnumerable<T> GetAll() => _store.AsReadOnly();

        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _store.Where(predicate).ToList();
        }

        public IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _store.AsQueryable().Where(predicate).ToList();
        }

        public PagedResult<T> GetPaged(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _store.AsQueryable();
            if (predicate is not null)
                query = query.Where(predicate);

            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
        }

        public void Add(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _store.Add(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _store.AddRange(entities);
        }

        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var index = _store.FindIndex(e => GetId(e) == GetId(entity));
            if (index >= 0)
                _store[index] = entity;
        }

        public void Delete(string id)
        {
            var entity = GetById(id);
            if (entity != null)
                _store.Remove(entity);
        }

        public bool Exists(string id) => _store.Any(e => GetId(e) == id);

        public int Count() => _store.Count;

        public int Count(Expression<Func<T, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            return _store.AsQueryable().Count(predicate);
        }
    }
}
