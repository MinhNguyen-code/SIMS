using System.Linq.Expressions;
using DayNeCu3726.Common;

namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository contract providing standard CRUD plus server-side querying.
    /// <para>
    /// Dependency Inversion Principle: services depend on this abstraction, never on
    /// <c>DbContext</c>, so the storage technology can be swapped (SQL Server, SQLite, in-memory
    /// test double) without touching business logic.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        T? GetById(string id);
        IEnumerable<T> GetAll();

        /// <summary>
        /// Filters using a delegate. Evaluated on the client, so it must only be used for small sets.
        /// <para>
        /// Prefer <see cref="Query"/> for anything that can grow: a <c>Func&lt;T,bool&gt;</c> cannot be
        /// translated to SQL, which forces the provider to load the entire table into memory first.
        /// </para>
        /// </summary>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Filters using an expression tree that the provider translates into a SQL WHERE clause,
        /// so only matching rows ever leave the database. Added to satisfy the Performance and
        /// Scalability non-functional requirements.
        /// </summary>
        IEnumerable<T> Query(Expression<Func<T, bool>> predicate);

        /// <summary>Returns one page of results, applying OFFSET/FETCH at the database rather than in memory.</summary>
        PagedResult<T> GetPaged(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null);

        void Add(T entity);

        /// <summary>Adds many entities in one call, avoiding a per-entity round-trip during bulk import.</summary>
        void AddRange(IEnumerable<T> entities);

        void Update(T entity);
        void Delete(string id);
        bool Exists(string id);
        int Count();

        /// <summary>Counts only the rows matching <paramref name="predicate"/>, evaluated server-side.</summary>
        int Count(Expression<Func<T, bool>> predicate);
    }
}
