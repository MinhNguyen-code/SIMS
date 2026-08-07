namespace DayNeCu3726.Common
{
    /// <summary>
    /// A single page of results plus the metadata a UI needs to render pagination controls.
    /// <para>
    /// Introduced to satisfy the Scalability and Performance non-functional requirements: screens
    /// that previously called <c>GetAll()</c> materialised every row in the table, so response time
    /// grew linearly with the size of the dataset. Paging keeps it bounded by <see cref="PageSize"/>.
    /// </para>
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public static PagedResult<T> Empty(int pageNumber, int pageSize) =>
            new(Array.Empty<T>(), pageNumber, pageSize, 0);
    }
}
