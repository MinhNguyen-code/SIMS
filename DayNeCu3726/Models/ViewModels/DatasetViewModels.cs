using System.ComponentModel.DataAnnotations;
using DayNeCu3726.DataProcessing.Pipeline;

namespace DayNeCu3726.Models.ViewModels
{
    /// <summary>
    /// Backs the bulk dataset screen: user-selected import options in, results out.
    /// </summary>
    public class DatasetImportViewModel
    {
        [Display(Name = "Batch size")]
        [Range(1, 50_000, ErrorMessage = "Batch size must be between 1 and 50,000.")]
        public int BatchSize { get; set; } = 1_000;

        [Display(Name = "Stop on first error")]
        public bool StopOnFirstError { get; set; }

        [Display(Name = "Validate only (no data is saved)")]
        public bool ValidateOnly { get; set; }

        [Display(Name = "Update records that already exist")]
        public bool UpdateExisting { get; set; }

        /// <summary>Populated after an import run; null before the form is submitted.</summary>
        public ImportResult? Result { get; set; }

        /// <summary>Populated after an analysis run; null before the form is submitted.</summary>
        public DatasetStatistics? Statistics { get; set; }
    }
}
