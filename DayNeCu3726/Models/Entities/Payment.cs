using System;

namespace DayNeCu3726.Models.Entities
{
    /// <summary>
    /// Payment record for tuition fee transactions.
    /// </summary>
    public class Payment
    {
        public string PaymentId { get; set; } = Guid.NewGuid().ToString();
        public string TuitionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // BankTransfer, Cash, MoMo, VNPay
        public string? TransactionCode { get; set; }
        public string? Note { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tuition? Tuition { get; set; }
    }
}
