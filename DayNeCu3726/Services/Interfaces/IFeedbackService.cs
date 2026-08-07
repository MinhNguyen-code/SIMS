using DayNeCu3726.Models.ViewModels;
using System.Collections.Generic;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IFeedbackService
    {
        IEnumerable<FeedbackViewModel> GetStudentFeedbacks(string studentId);
        (bool success, string message) SubmitFeedback(string studentId, CreateFeedbackViewModel model);
    }
}
