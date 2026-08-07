using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.ViewModels;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _uow;

        public AssignmentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IEnumerable<AssignmentViewModel> GetAssignmentsForStudent(string studentId)
        {
            var enrollments = _uow.Enrollments.Find(e => e.StudentId == studentId).ToList();
            var courseIds = enrollments.Select(e => e.CourseId).ToList();
            
            var assignments = _uow.Assignments.Find(a => courseIds.Contains(a.CourseId)).ToList();
            var submissions = _uow.Submissions.Find(s => s.StudentId == studentId).ToList();

            var result = new List<AssignmentViewModel>();
            foreach (var a in assignments)
            {
                var course = _uow.Courses.GetById(a.CourseId);
                var sub = submissions.FirstOrDefault(s => s.AssignmentId == a.AssignmentId);
                result.Add(new AssignmentViewModel
                {
                    AssignmentId = a.AssignmentId,
                    CourseId = a.CourseId,
                    CourseName = course?.Name ?? "",
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    CreatedAt = a.CreatedAt,
                    HasSubmitted = sub != null,
                    MyGrade = sub?.Grade
                });
            }
            return result.OrderByDescending(a => a.CreatedAt);
        }

        public IEnumerable<AssignmentViewModel> GetAssignmentsForFaculty(string facultyId)
        {
            var courses = _uow.Courses.Find(c => c.FacultyId == facultyId).ToList();
            var courseIds = courses.Select(c => c.CourseId).ToList();
            
            var assignments = _uow.Assignments.Find(a => courseIds.Contains(a.CourseId)).ToList();
            
            return MapToViewModels(assignments);
        }

        public IEnumerable<AssignmentViewModel> GetAllAssignments()
        {
            var assignments = _uow.Assignments.GetAll().ToList();
            return MapToViewModels(assignments);
        }

        private IEnumerable<AssignmentViewModel> MapToViewModels(List<Assignment> assignments)
        {
            var result = new List<AssignmentViewModel>();
            foreach (var a in assignments)
            {
                var course = _uow.Courses.GetById(a.CourseId);
                var subs = _uow.Submissions.Find(s => s.AssignmentId == a.AssignmentId).ToList();
                result.Add(new AssignmentViewModel
                {
                    AssignmentId = a.AssignmentId,
                    CourseId = a.CourseId,
                    CourseName = course?.Name ?? "",
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    CreatedAt = a.CreatedAt,
                    SubmissionCount = subs.Count,
                    GradedCount = subs.Count(s => s.Grade.HasValue)
                });
            }
            return result.OrderByDescending(a => a.CreatedAt);
        }

        public Assignment? GetAssignmentById(string id)
        {
            var a = _uow.Assignments.GetById(id);
            if (a != null) {
                a.Course = _uow.Courses.GetById(a.CourseId);
            }
            return a;
        }

        public void CreateAssignment(CreateAssignmentViewModel model)
        {
            var assignment = new Assignment
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Deadline = model.Deadline
            };
            _uow.Assignments.Add(assignment);
            _uow.SaveChanges();
        }

        public void SubmitAssignment(string assignmentId, string studentId, string filePath, string originalFileName)
        {
            var existing = _uow.Submissions.Find(s => s.AssignmentId == assignmentId && s.StudentId == studentId).FirstOrDefault();
            if (existing != null)
            {
                existing.FilePath = filePath;
                existing.OriginalFileName = originalFileName;
                existing.SubmittedAt = DateTime.UtcNow;
                _uow.Submissions.Update(existing);
            }
            else
            {
                var sub = new AssignmentSubmission
                {
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    FilePath = filePath,
                    OriginalFileName = originalFileName
                };
                _uow.Submissions.Add(sub);
            }
            _uow.SaveChanges();
        }

        public IEnumerable<SubmissionViewModel> GetSubmissionsForAssignment(string assignmentId)
        {
            var subs = _uow.Submissions.Find(s => s.AssignmentId == assignmentId).ToList();
            var result = new List<SubmissionViewModel>();
            foreach (var s in subs)
            {
                var student = _uow.Students.GetById(s.StudentId);
                result.Add(new SubmissionViewModel
                {
                    SubmissionId = s.SubmissionId,
                    AssignmentId = s.AssignmentId,
                    StudentId = s.StudentId,
                    StudentName = student?.FullName ?? "",
                    StudentCode = student?.StudentCode ?? "",
                    OriginalFileName = s.OriginalFileName,
                    FilePath = s.FilePath,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    Feedback = s.Feedback
                });
            }
            return result.OrderBy(s => s.SubmittedAt);
        }

        public SubmissionViewModel? GetSubmissionById(string submissionId)
        {
            var s = _uow.Submissions.GetById(submissionId);
            if (s == null) return null;
            
            var student = _uow.Students.GetById(s.StudentId);
            return new SubmissionViewModel
            {
                SubmissionId = s.SubmissionId,
                AssignmentId = s.AssignmentId,
                StudentId = s.StudentId,
                StudentName = student?.FullName ?? "",
                StudentCode = student?.StudentCode ?? "",
                OriginalFileName = s.OriginalFileName,
                FilePath = s.FilePath,
                SubmittedAt = s.SubmittedAt,
                Grade = s.Grade,
                Feedback = s.Feedback
            };
        }

        public void GradeSubmission(string submissionId, double grade, string feedback)
        {
            var sub = _uow.Submissions.GetById(submissionId);
            if (sub != null)
            {
                sub.Grade = grade;
                sub.Feedback = feedback;
                _uow.Submissions.Update(sub);
                _uow.SaveChanges();

                var assignment = _uow.Assignments.GetById(sub.AssignmentId);
                if (assignment != null)
                {
                    var enrollment = _uow.Enrollments.Find(e => e.CourseId == assignment.CourseId && e.StudentId == sub.StudentId).FirstOrDefault();
                    if (enrollment != null)
                    {
                        var courseAssignments = _uow.Assignments.Find(a => a.CourseId == assignment.CourseId).Select(a => a.AssignmentId).ToList();
                        var studentSubmissions = _uow.Submissions.Find(s => s.StudentId == sub.StudentId && courseAssignments.Contains(s.AssignmentId) && s.Grade.HasValue).ToList();
                        
                        if (studentSubmissions.Any())
                        {
                            enrollment.Grade = studentSubmissions.Average(s => s.Grade.Value);
                        }
                        _uow.Enrollments.Update(enrollment);
                        _uow.SaveChanges();
                    }
                }
            }
        }
    }
}
