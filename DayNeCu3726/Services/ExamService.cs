using System;
using System.Collections.Generic;
using System.Linq;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ExamViewModel> GetAllExams()
        {
            var exams = _unitOfWork.Exams.GetAll().ToList();
            return exams.Select(MapToViewModel);
        }

        public IEnumerable<ExamViewModel> GetExamsByCourse(string courseId)
        {
            var exams = _unitOfWork.Exams.GetByCourse(courseId).ToList();
            return exams.Select(MapToViewModel);
        }

        public ExamViewModel? GetExamById(string examId)
        {
            var exam = _unitOfWork.Exams.GetById(examId);
            if (exam == null) return null;
            return MapToViewModel(exam);
        }

        public (bool success, string message) CreateExam(CreateExamViewModel model)
        {
            var course = _unitOfWork.Courses.GetById(model.CourseId);
            if (course == null) return (false, "Course not found.");

            var exam = new Exam
            {
                ExamId = Guid.NewGuid().ToString(),
                CourseId = model.CourseId,
                ExamType = model.ExamType,
                ExamDate = model.ExamDate,
                TimeSlot = model.TimeSlot,
                Room = model.Room,
                SupervisorId = model.SupervisorId,
                Semester = model.Semester,
                Status = ExamStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                Course = course
            };

            // Assuming SupervisorId links to Faculty/User, can set SupervisorName here if needed.
            if (!string.IsNullOrEmpty(model.SupervisorId))
            {
                var faculty = _unitOfWork.Users.GetById(model.SupervisorId);
                if (faculty != null)
                {
                    exam.SupervisorName = faculty.FullName;
                }
            }

            _unitOfWork.Exams.Add(exam);
            _unitOfWork.SaveChanges();

            return (true, "Exam schedule created successfully.");
        }

        public (bool success, string message) UpdateExamStatus(string examId, ExamStatus status)
        {
            var exam = _unitOfWork.Exams.GetById(examId);
            if (exam == null) return (false, "Exam schedule not found.");

            exam.Status = status;
            _unitOfWork.Exams.Update(exam);
            _unitOfWork.SaveChanges();

            return (true, "Status updated successfully.");
        }

        public (bool success, string message) DeleteExam(string examId)
        {
            var exam = _unitOfWork.Exams.GetById(examId);
            if (exam == null) return (false, "Exam schedule not found.");

            _unitOfWork.Exams.Delete(examId);
            _unitOfWork.SaveChanges();

            return (true, "Exam schedule deleted.");
        }

        private ExamViewModel MapToViewModel(Exam exam)
        {
            var course = _unitOfWork.Courses.GetById(exam.CourseId);
            
            // Calculate eligible students: absences <= 6
            var enrollments = _unitOfWork.Enrollments.GetByCourse(exam.CourseId);
            int eligibleCount = enrollments.Count(e => e.Absences <= 6);

            return new ExamViewModel
            {
                ExamId = exam.ExamId,
                CourseId = exam.CourseId,
                CourseCode = course?.CourseCode ?? "",
                CourseName = course?.Name ?? "",
                ExamType = exam.ExamType,
                ExamDate = exam.ExamDate,
                TimeSlot = exam.TimeSlot,
                Room = exam.Room,
                SupervisorId = exam.SupervisorId,
                SupervisorName = exam.SupervisorName,
                Semester = exam.Semester,
                Status = exam.Status,
                EligibleStudentCount = eligibleCount
            };
        }
    }
}
