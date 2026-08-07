using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayNeCu3726.Services
{
    public class FeedbackService : IFeedbackService
    {
        private static readonly List<FeedbackViewModel> _feedbacks = new();

        public IEnumerable<FeedbackViewModel> GetStudentFeedbacks(string studentId)
        {
            if (!_feedbacks.Any())
            {
                return new List<FeedbackViewModel>
                {
                    new FeedbackViewModel
                    {
                        FeedbackId = Guid.NewGuid().ToString(),
                        CourseId = "C1",
                        CourseCode = "PRO101",
                        CourseName = "C# Programming",
                        FacultyName = "IT",
                        Semester = "Fall2023",
                        IsEvaluated = false
                    }
                };
            }
            
            return _feedbacks;
        }

        public (bool success, string message) SubmitFeedback(string studentId, CreateFeedbackViewModel model)
        {
            var feedback = new FeedbackViewModel
            {
                FeedbackId = Guid.NewGuid().ToString(),
                CourseId = model.CourseId,
                CourseCode = model.CourseCode,
                CourseName = model.CourseName,
                FacultyName = model.FacultyName,
                TeachingQuality = model.TeachingQuality,
                ContentRelevance = model.ContentRelevance,
                Communication = model.Communication,
                OverallRating = model.OverallRating,
                Comments = model.Comments,
                Semester = "Fall2023", 
                SubmittedAt = DateTime.Now,
                IsEvaluated = true
            };

            _feedbacks.Add(feedback);
            return (true, "Thank you for submitting your feedback.");
        }
    }
}
