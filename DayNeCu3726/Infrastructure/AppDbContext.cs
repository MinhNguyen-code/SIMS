using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Models.Entities;

namespace DayNeCu3726.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Faculty> Faculties { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Parent> Parents { get; set; } = null!;
        public DbSet<ServiceRequest> ServiceRequests { get; set; } = null!;
        public DbSet<Tuition> Tuitions { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Announcement> Announcements { get; set; } = null!;
        public DbSet<Exam> Exams { get; set; } = null!;
        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPH (Table-Per-Hierarchy) Inheritance for User hierarchy
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Student>("Student")
                .HasValue<Faculty>("Faculty")
                .HasValue<Admin>("Admin")
                .HasValue<Parent>("Parent");

            // Course configurations
            modelBuilder.Entity<Course>()
                .HasKey(c => c.CourseId);

            // Enrollment configurations
            modelBuilder.Entity<Enrollment>()
                .HasKey(e => e.EnrollmentId);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Parent -> Student relationship
            modelBuilder.Entity<Parent>()
                .HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // ServiceRequest configurations
            modelBuilder.Entity<ServiceRequest>()
                .HasKey(sr => sr.RequestId);
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Student)
                .WithMany()
                .HasForeignKey(sr => sr.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Tuition configurations
            modelBuilder.Entity<Tuition>()
                .HasKey(t => t.TuitionId);
            modelBuilder.Entity<Tuition>()
                .Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Tuition>()
                .Property(t => t.PaidAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Tuition>()
                .Property(t => t.CostPerCourse).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Tuition>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment configurations
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId);
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Tuition)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TuitionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Feedback configurations
            modelBuilder.Entity<Feedback>()
                .HasKey(f => f.FeedbackId);
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Student)
                .WithMany()
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Announcement configurations
            modelBuilder.Entity<Announcement>()
                .HasKey(a => a.AnnouncementId);
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // Exam configurations
            modelBuilder.Entity<Exam>()
                .HasKey(e => e.ExamId);
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            // Assignment configurations
            modelBuilder.Entity<Assignment>()
                .HasKey(a => a.AssignmentId);
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssignmentSubmission configurations
            modelBuilder.Entity<AssignmentSubmission>()
                .HasKey(s => s.SubmissionId);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed property
            modelBuilder.Entity<Tuition>()
                .Ignore(t => t.RemainingAmount);
        }
    }
}
