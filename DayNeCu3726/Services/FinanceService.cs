using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayNeCu3726.Services
{
    public class FinanceService : IFinanceService
    {
        private static readonly List<TuitionViewModel> _tuitions = new();

        public IEnumerable<TuitionViewModel> GetTuitionByStudent(string studentId)
        {
            return _tuitions.Where(t => t.StudentId == studentId);
        }

        public TuitionViewModel? GetTuitionById(string tuitionId)
        {
            return _tuitions.FirstOrDefault(t => t.TuitionId == tuitionId);
        }

        public (bool success, string message) ProcessPayment(string tuitionId, decimal amount, string method, string? note)
        {
            var tuition = _tuitions.FirstOrDefault(t => t.TuitionId == tuitionId);
            if (tuition == null)
            {
                return (false, "Tuition information not found.");
            }

            if (amount <= 0)
            {
                return (false, "Payment amount must be greater than 0.");
            }

            if (amount > tuition.RemainingAmount)
            {
                return (false, "Payment amount cannot be greater than the remaining balance.");
            }

            var payment = new PaymentViewModel
            {
                PaymentId = Guid.NewGuid().ToString(),
                TuitionId = tuitionId,
                Amount = amount,
                PaymentMethod = method,
                TransactionCode = $"TXN{DateTime.Now.Ticks}",
                Note = note,
                PaymentDate = DateTime.Now
            };

            tuition.Payments.Add(payment);
            tuition.PaidAmount += amount;

            if (tuition.PaidAmount >= tuition.TotalAmount)
            {
                tuition.Status = TuitionStatus.Paid;
            }
            else
            {
                tuition.Status = TuitionStatus.PartiallyPaid;
            }

            return (true, "Payment successful.");
        }

        public (bool success, string message) RecalculateTuition(string studentId, string semester)
        {
            var existing = _tuitions.FirstOrDefault(t => t.StudentId == studentId && t.Semester == semester);
            if (existing != null)
            {
                existing.CourseCount = 5; 
                existing.TotalAmount = existing.CourseCount * existing.CostPerCourse;
                if (existing.PaidAmount >= existing.TotalAmount)
                    existing.Status = TuitionStatus.Paid;
                else if (existing.PaidAmount > 0)
                    existing.Status = TuitionStatus.PartiallyPaid;
                else
                    existing.Status = TuitionStatus.Unpaid;
                    
                return (true, "Tuition updated successfully.");
            }

            var newTuition = new TuitionViewModel
            {
                TuitionId = Guid.NewGuid().ToString(),
                StudentId = studentId,
                StudentName = "Student Name",
                StudentCode = "STU123",
                Semester = semester,
                CourseCount = 5,
                CostPerCourse = 4500000m,
                PaidAmount = 0,
                Status = TuitionStatus.Unpaid,
                DueDate = DateTime.Now.AddDays(30)
            };
            newTuition.TotalAmount = newTuition.CourseCount * newTuition.CostPerCourse;
            
            _tuitions.Add(newTuition);
            return (true, "Tuition calculated for the new semester.");
        }
    }
}
