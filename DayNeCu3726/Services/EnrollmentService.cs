using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Patterns.Observer;
using DayNeCu3726.Patterns.Singleton;
using DayNeCu3726.Patterns.Strategy;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Services.Interfaces;

namespace DayNeCu3726.Services
{
    /// <summary>
    /// Enrollment Service – manages student-course enrollments.
    /// Uses Observer Pattern to notify on enrollment events.
    /// Uses Strategy Pattern for grade calculation.
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EnrollmentEventPublisher _publisher;
        private readonly IGradeStrategy _gradeStrategy;

        /// <summary>
        /// Collaborators are injected rather than constructed internally.
        /// <para>
        /// The previous constructor called <c>new EnrollmentEventPublisher()</c> and
        /// <c>new EmailNotificationObserver()</c> directly. That hard-wired the service to those
        /// concrete classes (a Dependency Inversion Principle violation), created a fresh publisher
        /// on every request so subscriptions could never be configured centrally, and made unit
        /// testing impossible without triggering real notification side effects.
        /// </para>
        /// <para>
        /// The optional parameters keep every existing call site compiling while still allowing the
        /// container — or a test — to supply its own publisher and grading strategy.
        /// </para>
        /// </summary>
        public EnrollmentService(
            IUnitOfWork unitOfWork,
            EnrollmentEventPublisher? publisher = null,
            IGradeStrategy? gradeStrategy = null)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _publisher = publisher ?? CreateDefaultPublisher();
            _gradeStrategy = gradeStrategy ?? GradeStrategyFactory.Create(SystemConfiguration.Instance.GradingScheme);
        }

        private static EnrollmentEventPublisher CreateDefaultPublisher()
        {
            var publisher = new EnrollmentEventPublisher();
            publisher.Subscribe(new EmailNotificationObserver());
            publisher.Subscribe(new AuditLogObserver());
            return publisher;
        }

        public IEnumerable<Enrollment> GetEnrollmentsByStudent(string studentId)
        {
            var enrollments = _unitOfWork.Enrollments.GetByStudent(studentId).ToList();
            // Populate navigation properties
            foreach (var e in enrollments)
            {
                e.Student ??= _unitOfWork.Students.GetById(e.StudentId);
                e.Course ??= _unitOfWork.Courses.GetById(e.CourseId);
            }
            return enrollments;
        }

        public IEnumerable<Enrollment> GetEnrollmentsByCourse(string courseId)
        {
            var enrollments = _unitOfWork.Enrollments.GetByCourse(courseId).ToList();
            foreach (var e in enrollments)
            {
                e.Student ??= _unitOfWork.Students.GetById(e.StudentId);
                e.Course ??= _unitOfWork.Courses.GetById(e.CourseId);
            }
            return enrollments;
        }

        public (bool success, string message) EnrollStudent(string studentId, string courseId)
        {
            var student = _unitOfWork.Students.GetById(studentId);
            if (student == null)
                return (false, "Student not found.");

            var course = _unitOfWork.Courses.GetById(courseId);
            if (course == null)
                return (false, "Course not found.");

            if (course.Status != Models.Enums.CourseStatus.Active)
                return (false, "Course is not currently active.");

            if (!course.HasCapacity)
                return (false, "Course has reached maximum enrollment capacity.");

            if (_unitOfWork.Enrollments.IsStudentEnrolled(studentId, courseId))
                return (false, "Student is already enrolled in this course.");

            // Check max courses per student
            var currentEnrollments = _unitOfWork.Enrollments
                .GetByStudent(studentId)
                .Count(e => e.Status == EnrollmentStatus.Enrolled);

            if (currentEnrollments >= SystemConfiguration.Instance.MaxCoursesPerStudent)
                return (false, $"Student cannot enroll in more than {SystemConfiguration.Instance.MaxCoursesPerStudent} courses.");

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Enrolled
            };

            _unitOfWork.Enrollments.Add(enrollment);
            course.CurrentEnrollment++;
            _unitOfWork.Courses.Update(course);
            _unitOfWork.SaveChanges();

            // Notify observers
            _publisher.NotifyEnrolled(student, course);

            return (true, $"Successfully enrolled in '{course.Name}'.");
        }

        public (bool success, string message) DropCourse(string studentId, string courseId)
        {
            var enrollment = _unitOfWork.Enrollments.GetByStudentAndCourse(studentId, courseId);
            if (enrollment == null)
                return (false, "Enrollment not found.");

            if (enrollment.Status != EnrollmentStatus.Enrolled)
                return (false, "Cannot drop a course that is not actively enrolled.");

            enrollment.Status = EnrollmentStatus.Dropped;
            _unitOfWork.Enrollments.Update(enrollment);

            var course = _unitOfWork.Courses.GetById(courseId);
            if (course != null)
            {
                course.CurrentEnrollment = Math.Max(0, course.CurrentEnrollment - 1);
                _unitOfWork.Courses.Update(course);
            }

            _unitOfWork.SaveChanges();

            var student = _unitOfWork.Students.GetById(studentId);
            if (student != null && course != null)
                _publisher.NotifyDropped(student, course);

            return (true, "Course dropped successfully.");
        }

        public (bool success, string message) UpdateGrade(string enrollmentId, double grade, int absences, string? remarks = null)
        {
            if (grade < 0 || grade > 10)
                return (false, "Grade must be between 0 and 10.");

            if (absences < 0 || absences > 30)
                return (false, "Absences must be between 0 and 30.");

            var enrollment = _unitOfWork.Enrollments.GetById(enrollmentId);
            if (enrollment == null)
                return (false, "Enrollment not found.");

            enrollment.Grade = grade;
            enrollment.Absences = absences;
            enrollment.Remarks = remarks;

            // BTEC rule: max 6 absences allowed out of 30 sessions.
            if (absences > 6)
            {
                enrollment.LetterGrade = "F";
                enrollment.Status = EnrollmentStatus.Failed;
            }
            else
            {
                enrollment.LetterGrade = _gradeStrategy.CalculateLetterGrade(grade);
                enrollment.Status = _gradeStrategy.IsPassing(grade)
                    ? EnrollmentStatus.Completed
                    : EnrollmentStatus.Failed;
            }

            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.SaveChanges();

            // Update student GPA
            RecalculateGPA(enrollment.StudentId);

            var student = _unitOfWork.Students.GetById(enrollment.StudentId);
            var course = _unitOfWork.Courses.GetById(enrollment.CourseId);
            if (student != null && course != null)
                _publisher.NotifyGradeUpdated(student, course, grade);

            return (true, "Grade and attendance updated successfully.");
        }

        public (bool success, string message) UpdateAttendance(string enrollmentId, string attendancePattern)
        {
            if (string.IsNullOrEmpty(attendancePattern) || attendancePattern.Length != 30)
                return (false, "Attendance pattern must be exactly 30 characters long.");

            var enrollment = _unitOfWork.Enrollments.GetById(enrollmentId);
            if (enrollment == null)
                return (false, "Enrollment not found.");

            enrollment.AttendancePattern = attendancePattern;
            
            // Recalculate absences count by counting 'A' characters
            int absences = attendancePattern.Count(c => c == 'A');
            enrollment.Absences = absences;

            // If absences exceed BTEC limit, student fails
            if (absences > 6)
            {
                enrollment.LetterGrade = "F";
                enrollment.Status = EnrollmentStatus.Failed;
            }
            else if (enrollment.Grade.HasValue)
            {
                // Reapply grading strategy since status might change from Failed back to Completed
                enrollment.LetterGrade = _gradeStrategy.CalculateLetterGrade(enrollment.Grade.Value);
                enrollment.Status = _gradeStrategy.IsPassing(enrollment.Grade.Value)
                    ? EnrollmentStatus.Completed
                    : EnrollmentStatus.Failed;
            }

            _unitOfWork.Enrollments.Update(enrollment);
            _unitOfWork.SaveChanges();

            return (true, "Attendance record updated successfully.");
        }

        public bool IsStudentEnrolled(string studentId, string courseId)
            => _unitOfWork.Enrollments.IsStudentEnrolled(studentId, courseId);

        public int GetTotalEnrollments()
            => _unitOfWork.Enrollments.Count();

        public IEnumerable<Enrollment> GetRecentEnrollments(int count = 5)
            => _unitOfWork.Enrollments.GetAll()
                .OrderByDescending(e => e.EnrollDate)
                .Take(count);

        /// <summary>
        /// Recalculates a student's academic average from their graded enrollments.
        /// <para>
        /// <b>Defect fixed:</b> under the BTEC scheme this method previously assigned
        /// <c>student.GPA = 0.0</c> with the comment "BTEC doesn't calculate GPA". BTEC does not use
        /// a 4-point GPA, but the consequence was that every dashboard and student record displayed
        /// "GPA 0.0" even for students who had completed courses with marks of 9.0 — the figure
        /// looked like a calculation bug and made the metric worthless.
        /// </para>
        /// <para>
        /// The average mark on the 0–10 scale is meaningful under every scheme, so it is now always
        /// computed. The views label it "Average Mark" for BTEC and "GPA" otherwise, which is
        /// accurate for both.
        /// </para>
        /// </summary>
        private void RecalculateGPA(string studentId)
        {
            var student = _unitOfWork.Students.GetById(studentId);
            if (student == null) return;

            student.GPA = CalculateAverageMark(studentId);

            _unitOfWork.Students.Update(student);
            _unitOfWork.Users.Update(student);
        }

        /// <summary>
        /// Mean of all recorded marks for a student, rounded to two decimals.
        /// Returns 0 when the student has no graded enrollment yet.
        /// </summary>
        public double CalculateAverageMark(string studentId)
        {
            var gradedMarks = _unitOfWork.Enrollments
                .GetByStudent(studentId)
                .Where(e => e.Grade.HasValue)
                .Select(e => e.Grade!.Value)
                .ToList();

            return gradedMarks.Count == 0
                ? 0.0
                : Math.Round(gradedMarks.Average(), 2);
        }
    }
}
