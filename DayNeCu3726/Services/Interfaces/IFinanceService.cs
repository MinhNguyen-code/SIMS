using DayNeCu3726.Models.ViewModels;
using System.Collections.Generic;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IFinanceService
    {
        IEnumerable<TuitionViewModel> GetTuitionByStudent(string studentId);
        TuitionViewModel? GetTuitionById(string tuitionId);
        (bool success, string message) ProcessPayment(string tuitionId, decimal amount, string method, string? note);
        (bool success, string message) RecalculateTuition(string studentId, string semester);
    }
}
