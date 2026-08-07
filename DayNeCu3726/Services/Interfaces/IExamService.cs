using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;

namespace DayNeCu3726.Services.Interfaces
{
    public interface IExamService
    {
        IEnumerable<ExamViewModel> GetAllExams();
        IEnumerable<ExamViewModel> GetExamsByCourse(string courseId);
        ExamViewModel? GetExamById(string examId);
        (bool success, string message) CreateExam(CreateExamViewModel model);
        (bool success, string message) UpdateExamStatus(string examId, ExamStatus status);
        (bool success, string message) DeleteExam(string examId);
    }
}
