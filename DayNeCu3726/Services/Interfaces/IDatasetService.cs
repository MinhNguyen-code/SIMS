using DayNeCu3726.DataProcessing.Pipeline;

namespace DayNeCu3726.Services.Interfaces
{
    /// <summary>
    /// Application-level entry point for large-dataset operations on the student CSV store.
    /// <para>
    /// Single Responsibility Principle: bulk dataset work is a separate concern from the per-record
    /// CRUD handled by <see cref="IStudentService"/>, so it gets its own contract rather than being
    /// bolted onto an already broad interface.
    /// </para>
    /// </summary>
    public interface IDatasetService
    {
        /// <summary>Streams a CSV file into the database in batches and reports per-row outcomes.</summary>
        Task<ImportResult> ImportStudentsAsync(Stream csvSource, ImportOptions options, CancellationToken cancellationToken = default);

        /// <summary>Streams every student record out as CSV; returns the number of rows written.</summary>
        Task<int> ExportStudentsAsync(Stream destination, CancellationToken cancellationToken = default);

        /// <summary>Computes aggregate statistics over a CSV dataset in one pass without loading it into memory.</summary>
        Task<DatasetStatistics> AnalyzeAsync(Stream csvSource, CancellationToken cancellationToken = default);

        /// <summary>Writes a header-only CSV that shows users the exact expected import format.</summary>
        Task WriteImportTemplateAsync(Stream destination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a synthetic dataset of the requested size, used to demonstrate and benchmark
        /// the application against a genuinely large volume of records.
        /// </summary>
        Task<int> GenerateSampleDatasetAsync(Stream destination, int recordCount, CancellationToken cancellationToken = default);
    }
}
