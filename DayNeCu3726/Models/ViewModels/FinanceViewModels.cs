using DayNeCu3726.Models.Enums;

namespace DayNeCu3726.Models.ViewModels
{
    public class TuitionViewModel
    {
        public string TuitionId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public int CourseCount { get; set; }
        public decimal CostPerCourse { get; set; } = 4_500_000m;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;
        public TuitionStatus Status { get; set; }
        public string StatusBadgeClass => Status switch
        {
            TuitionStatus.Paid => "bg-success text-white",
            TuitionStatus.PartiallyPaid => "bg-info text-white",
            TuitionStatus.Unpaid => "bg-warning text-dark",
            TuitionStatus.Overdue => "bg-danger text-white",
            _ => "bg-secondary text-white"
        };
        public string StatusText => Status switch
        {
            TuitionStatus.Paid => "Paid",
            TuitionStatus.PartiallyPaid => "Partially Paid",
            TuitionStatus.Unpaid => "Unpaid",
            TuitionStatus.Overdue => "Overdue",
            _ => "Unknown"
        };
        public DateTime DueDate { get; set; }
        public List<PaymentViewModel> Payments { get; set; } = new();
    }

    public class PaymentViewModel
    {
        public string PaymentId { get; set; } = string.Empty;
        public string TuitionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionCode { get; set; }
        public string? Note { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class PayTuitionViewModel
    {
        public string TuitionId { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public decimal RemainingAmount { get; set; }
        public decimal AmountToPay { get; set; }
        public string PaymentMethod { get; set; } = "BankTransfer";
        public string? Note { get; set; }
    }
}
