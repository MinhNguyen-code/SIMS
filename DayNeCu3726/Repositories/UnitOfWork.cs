using DayNeCu3726.Infrastructure;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Repositories.EF;

namespace DayNeCu3726.Repositories
{
    /// <summary>
    /// Unit of Work implementation – groups all repositories into one
    /// coordinated unit, ensuring consistency in data operations.
    /// Pattern: Unit of Work
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed = false;

        public IStudentRepository Students { get; }
        public ICourseRepository Courses { get; }
        public IEnrollmentRepository Enrollments { get; }
        public IUserRepository Users { get; }
        public IParentRepository Parents { get; }
        public IServiceRequestRepository ServiceRequests { get; }
        public ITuitionRepository Tuitions { get; }
        public IPaymentRepository Payments { get; }
        public IFeedbackRepository Feedbacks { get; }
        public IAnnouncementRepository Announcements { get; }
        public IExamRepository Exams { get; }
        public IAssignmentRepository Assignments { get; }
        public ISubmissionRepository Submissions { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Students = new EFStudentRepository(context);
            Courses = new EFCourseRepository(context);
            Enrollments = new EFEnrollmentRepository(context);
            Users = new EFUserRepository(context);
            Parents = new EFParentRepository(context);
            ServiceRequests = new EFServiceRequestRepository(context);
            Tuitions = new EFTuitionRepository(context);
            Payments = new EFPaymentRepository(context);
            Feedbacks = new EFFeedbackRepository(context);
            Announcements = new EFAnnouncementRepository(context);
            Exams = new EFExamRepository(context);
            Assignments = new EFAssignmentRepository(context);
            Submissions = new EFSubmissionRepository(context);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
