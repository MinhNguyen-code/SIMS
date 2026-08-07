namespace DayNeCu3726.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work pattern – coordinates all repository operations
    /// and ensures transactional consistency across the in-memory store.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        ICourseRepository Courses { get; }
        IEnrollmentRepository Enrollments { get; }
        IUserRepository Users { get; }
        IParentRepository Parents { get; }
        IServiceRequestRepository ServiceRequests { get; }
        ITuitionRepository Tuitions { get; }
        IPaymentRepository Payments { get; }
        IFeedbackRepository Feedbacks { get; }
        IAnnouncementRepository Announcements { get; }
        IExamRepository Exams { get; }
        IAssignmentRepository Assignments { get; }
        ISubmissionRepository Submissions { get; }
        void SaveChanges();
    }
}
