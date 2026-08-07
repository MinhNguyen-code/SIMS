using DayNeCu3726.DataProcessing.Pipeline;
using DayNeCu3726.Infrastructure.Authorization;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DayNeCu3726.Controllers
{
    /// <summary>
    /// User-facing entry point for the large-dataset CSV features: bulk import, streaming export,
    /// dataset analysis and sample-data generation.
    /// <para>
    /// Access is granted declaratively with <see cref="AuthorizeRoleAttribute"/> instead of the
    /// hand-written session checks repeated in the older controllers, so the rule is stated once and
    /// applies to every action in the class.
    /// </para>
    /// <para>
    /// The controller only translates HTTP to service calls; all processing lives in
    /// <see cref="IDatasetService"/>, keeping it thin and testable (Single Responsibility Principle).
    /// </para>
    /// </summary>
    [AuthorizeRole(UserRole.Admin, UserRole.Faculty)]
    public class DatasetController : Controller
    {
        /// <summary>Upper bound on upload size, protecting the server from an unbounded request body.</summary>
        private const long MaxUploadBytes = 512L * 1024 * 1024;   // 512 MB

        private const int MaxGeneratedRecords = 500_000;

        private readonly IDatasetService _datasetService;
        private readonly ILogger<DatasetController> _logger;

        public DatasetController(IDatasetService datasetService, ILogger<DatasetController> logger)
        {
            _datasetService = datasetService ?? throw new ArgumentNullException(nameof(datasetService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Index() => View(new DatasetImportViewModel());

        /// <summary>Streams an uploaded CSV into the database in batches and shows a per-row report.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<IActionResult> Import(DatasetImportViewModel model, IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError(nameof(file), "Please choose a CSV file to upload.");
                return View(nameof(Index), model);
            }

            if (!IsCsv(file.FileName))
            {
                ModelState.AddModelError(nameof(file), "Only .csv files are accepted.");
                return View(nameof(Index), model);
            }

            var options = new ImportOptionsBuilder()
                .WithBatchSize(model.BatchSize)
                .StopOnFirstError(model.StopOnFirstError)
                .ValidateOnly(model.ValidateOnly)
                .UpdateExisting(model.UpdateExisting)
                .WithMaxReportedErrors(200)
                .Build();

            try
            {
                // OpenReadStream is never fully buffered, so memory stays flat no matter the file size.
                await using var uploadStream = file.OpenReadStream();
                model.Result = await _datasetService.ImportStudentsAsync(uploadStream, options, cancellationToken);

                _logger.LogInformation("CSV import finished: {Summary}", model.Result.Summary());
            }
            catch (InvalidDataException ex)
            {
                // Thrown when required columns are missing — a user error, not a server fault.
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (OperationCanceledException)
            {
                ModelState.AddModelError(string.Empty, "The import was cancelled before it completed.");
            }

            return View(nameof(Index), model);
        }

        /// <summary>Runs a single streaming pass over an uploaded file and reports aggregate statistics.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxUploadBytes)]
        public async Task<IActionResult> Analyze(DatasetImportViewModel model, IFormFile? file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError(nameof(file), "Please choose a CSV file to analyse.");
                return View(nameof(Index), model);
            }

            await using var uploadStream = file.OpenReadStream();
            model.Statistics = await _datasetService.AnalyzeAsync(uploadStream, cancellationToken);

            return View(nameof(Index), model);
        }

        /// <summary>
        /// Streams every student to the client as CSV.
        /// Writing straight to <c>Response.Body</c> means the file is never held in memory or on disk.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers.ContentDisposition = $"attachment; filename=students-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

            var rowsWritten = await _datasetService.ExportStudentsAsync(Response.Body, cancellationToken);
            _logger.LogInformation("Exported {RowCount} student rows to CSV.", rowsWritten);

            return new EmptyResult();
        }

        /// <summary>Downloads a header-only CSV so users can see the exact expected import format.</summary>
        [HttpGet]
        public async Task<IActionResult> Template(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers.ContentDisposition = "attachment; filename=student-import-template.csv";

            await _datasetService.WriteImportTemplateAsync(Response.Body, cancellationToken);
            return new EmptyResult();
        }

        /// <summary>
        /// Generates a synthetic dataset of the requested size so the application can be demonstrated
        /// and benchmarked against a genuinely large volume of records.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GenerateSample(int recordCount = 50_000, CancellationToken cancellationToken = default)
        {
            recordCount = Math.Clamp(recordCount, 1, MaxGeneratedRecords);

            Response.ContentType = "text/csv; charset=utf-8";
            Response.Headers.ContentDisposition = $"attachment; filename=sample-students-{recordCount}.csv";

            await _datasetService.GenerateSampleDatasetAsync(Response.Body, recordCount, cancellationToken);
            return new EmptyResult();
        }

        private static bool IsCsv(string fileName) =>
            Path.GetExtension(fileName).Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }
}
