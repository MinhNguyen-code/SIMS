using DayNeCu3726.Models.Entities;
using DayNeCu3726.Models.Enums;
using DayNeCu3726.Patterns.Factory;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;

namespace DayNeCu3726.Infrastructure
{
    /// <summary>
    /// Seeds demo data into in-memory repositories on application startup.
    /// Fully configured with BTEC computing units.
    /// </summary>
    public static class DataSeeder
    {
        public static void Seed(IUnitOfWork uow)
        {
            // Only seed if empty
            if (uow.Users.GetAll().Any()) return;

            // Seed accounts are hashed with the same PBKDF2 hasher the application uses at runtime,
            // so no demo account is left protected by the superseded SHA-256 scheme.
            var passwordHasher = new Pbkdf2PasswordHasher();

            // Each distinct password is hashed once and reused: PBKDF2 is intentionally slow, and
            // hashing it per seeded record added seconds to every cold start.
            var hashCache = new Dictionary<string, string>(StringComparer.Ordinal);
            var hash = (string pw) =>
            {
                if (!hashCache.TryGetValue(pw, out var cached))
                {
                    cached = passwordHasher.Hash(pw);
                    hashCache[pw] = cached;
                }
                return cached;
            };

            // ─── ADMIN ────────────────────────────────────────────────────
            var admin = new Admin
            {
                Id = "admin-001",
                FullName = "System Administrator",
                Email = "admin@sims.edu",
                PasswordHash = hash("Admin@123"),
                AdminCode = "ADM2024001",
                PhoneNumber = "0901234567"
            };
            uow.Users.Add(admin);

            // ─── FACULTY ─────────────────────────────────────────────────
            var faculty5 = new Faculty
            {
                Id = "fac-005", FullName = "Dr. Vu Hoang Long", Email = "faculty5@sims.edu",
                PasswordHash = hash("Faculty@123"), FacultyCode = "FAC2024005",
                Department = "Computer Science", Position = "Assistant Professor",
                Specialization = "Machine Learning", PhoneNumber = "0956789012"
            };
            uow.Users.Add(faculty5);

            // ─── STUDENTS ─────────────────────────────────────────────────
            var students = new List<Student>();
            var rStu = new Random(42);

            // Special student for demo
            var minh = new Student
            {
                Id = "stu-000",
                FullName = "Nguyen Binh Minh",
                Email = "minh@sims.edu",
                PasswordHash = hash("Student@123"),
                StudentCode = "BH00000",
                DateOfBirth = new DateTime(2004, 5, 15),
                Gender = "Male",
                Program = "Computer Science (BTEC)",
                Department = "SE08102",
                EnrollmentYear = 2024,
                GPA = 0.0,
                AcademicStatus = AcademicStatus.Active,
                PhoneNumber = "0934567890"
            };
            students.Add(minh);
            uow.Students.Add(minh);
            uow.Users.Add(minh);

            var vinh = new Student
            {
                Id = "stu-001",
                FullName = "Tran The Vinh",
                Email = "vinh@sims.edu",
                PasswordHash = hash("Student@123"),
                StudentCode = "BH00001",
                DateOfBirth = new DateTime(2004, 1, 1),
                Gender = "Male",
                Program = "Computer Science (BTEC)",
                Department = "SE08102",
                EnrollmentYear = 2024,
                GPA = 0.0,
                AcademicStatus = AcademicStatus.Active,
                PhoneNumber = "0911111111"
            };
            students.Add(vinh);
            uow.Students.Add(vinh);
            uow.Users.Add(vinh);


            // ─── COURSES (BTEC COMPUTING PROGRAM) ──────────────────────────
            var courses = new List<Course>
            {
                // Spring 2025: Mon Slot 3-4, Wed Slot 1-2
                new() { CourseId = "crs-7388", CourseCode = "7388", Name = "Programming",
                    Description = "Unit 1: Programming. Basics of coding, design paradigms, algorithms, OOP concept and implementation.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 40, CurrentEnrollment = 20, Schedule = "Monday (12:00 - 16:10)",
                    DayPattern = "Mon", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Spring 2025", Status = CourseStatus.Active },

                new() { CourseId = "crs-7393", CourseCode = "7393", Name = "Networking",
                    Description = "Unit 2: Networking. Network architectures, protocols, standards, configuration and management.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 35, CurrentEnrollment = 20, Schedule = "Wednesday (07:15 - 11:25)",
                    DayPattern = "Wed", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Spring 2025", Status = CourseStatus.Active },

                // Summer 2025: Mon Slot 3-4, Wed Slot 1-2, Fri Slot 5-6
                new() { CourseId = "crs-7407", CourseCode = "7407", Name = "Planning a Computing Project",
                    Description = "Unit 6: Planning a Computing Project. Real-world project scope, resource planning, task scheduling and execution.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 30, CurrentEnrollment = 20, Schedule = "Monday (12:00 - 16:10)",
                    DayPattern = "Mon", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Summer 2025", Status = CourseStatus.Active },

                new() { CourseId = "crs-7398", CourseCode = "7398", Name = "Professional Practice",
                    Description = "Unit 3: Professional Practice. Interpersonal skills, collaboration, communication, professional standards in IT.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 40, CurrentEnrollment = 20, Schedule = "Wednesday (07:15 - 11:25)",
                    DayPattern = "Wed", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Summer 2025", Status = CourseStatus.Active },

                new() { CourseId = "crs-7400", CourseCode = "7400", Name = "Database Design & Development",
                    Description = "Unit 4: Database Design & Development. Relational database design, SQL queries, normalization, DBMS implementations.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 30, CurrentEnrollment = 20, Schedule = "Friday (16:20 - 20:30)",
                    DayPattern = "Fri", SlotGroup = 3,
                    Classroom = "SE08102", Semester = "Summer 2025", Status = CourseStatus.Active },

                // Fall 2025: Mon Slot 3-4, Wed Slot 1-2, Fri Slot 5-6
                new() { CourseId = "crs-7406", CourseCode = "7406", Name = "Security",
                    Description = "Unit 5: Security. Cyber threats, authentication protocols, firewalls, secure programming guidelines, encryption.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 35, CurrentEnrollment = 20, Schedule = "Monday (12:00 - 16:10)",
                    DayPattern = "Mon", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Fall 2025", Status = CourseStatus.Active },

                new() { CourseId = "crs-7430", CourseCode = "7430", Name = "Data Structures & Algorithms",
                    Description = "Unit 19: Data Structures & Algorithms. Binary trees, searching algorithms, sorting methods, complexity analysis.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 40, CurrentEnrollment = 20, Schedule = "Wednesday (07:15 - 11:25)",
                    DayPattern = "Wed", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Fall 2025", Status = CourseStatus.Active },

                new() { CourseId = "crs-7481", CourseCode = "7481", Name = "Internet of Things",
                    Description = "Unit 43: Internet of Things. IoT ecosystems, hardware prototyping, sensor data aggregation and cloud integration.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 50, CurrentEnrollment = 20, Schedule = "Friday (16:20 - 20:30)",
                    DayPattern = "Fri", SlotGroup = 3,
                    Classroom = "SE08102", Semester = "Fall 2025", Status = CourseStatus.Active },

                // Spring 2026 (Semester 4): Completed BTEC courses
                new() { CourseId = "crs-7428", CourseCode = "7428", Name = "Business Process Support",
                    Description = "Unit 12: Business Process Support. Business workflow analysis, ERP systems, support methodologies.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 35, CurrentEnrollment = 20, Schedule = "Monday (12:00 - 14:00)",
                    DayPattern = "Mon", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Spring 2026", Status = CourseStatus.Active },

                new() { CourseId = "crs-7408", CourseCode = "7408", Name = "Software Development Life Cycle",
                    Description = "Unit 9: Software Development Life Cycle. Agile, Waterfall, requirements gathering, unit testing, software deployment.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 30, CurrentEnrollment = 20, Schedule = "Wednesday (07:15 - 11:25)",
                    DayPattern = "Wed", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Spring 2026", Status = CourseStatus.Active },

                new() { CourseId = "crs-7419", CourseCode = "7419", Name = "Website Design & Development",
                    Description = "Unit 10: Website Design & Development. HTML5, CSS3, JS, responsive layout design, usability testing.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 30, CurrentEnrollment = 20, Schedule = "Friday (16:20 - 20:30)",
                    DayPattern = "Fri", SlotGroup = 3,
                    Classroom = "SE08102", Semester = "Spring 2026", Status = CourseStatus.Active },

                // Summer 2026 (Semester 5): Active/in-progress courses
                new() { CourseId = "crs-4902", CourseCode = "4902", Name = "Applied Programming and Design Principles",
                    Description = "Unit 17: Applied Programming and Design Principles.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 35, CurrentEnrollment = 20, Schedule = "Friday (12:00 - 14:00)",
                    DayPattern = "Fri", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Summer 2026", Status = CourseStatus.Active },

                new() { CourseId = "crs-7436", CourseCode = "7436", Name = "Application Development",
                    Description = "Unit 18: Application Development.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 40, CurrentEnrollment = 20, Schedule = "Thursday (07:15 - 11:25)",
                    DayPattern = "Thu", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Summer 2026", Status = CourseStatus.Active },

                new() { CourseId = "crs-7429", CourseCode = "7429", Name = "Discrete Maths",
                    Description = "Unit 19: Discrete Maths.",
                    Credits = 3, FacultyId = "fac-005", FacultyName = "Dr. Vu Hoang Long",
                    MaxEnrollment = 30, CurrentEnrollment = 20, Schedule = "Tuesday (12:00 - 14:00)",
                    DayPattern = "Tue", SlotGroup = 2,
                    Classroom = "SE08102", Semester = "Summer 2026", Status = CourseStatus.Active },

                // Fall 2026 (Semester 6): Future courses
                new() { CourseId = "crs-7425", CourseCode = "7425", Name = "Computer Research Project (Pearson Set)",
                    Description = "Unit 20: Computer Research Project (Pearson Set).",
                    Credits = 6, FacultyId = null, FacultyName = "",
                    MaxEnrollment = 30, CurrentEnrollment = 0, Schedule = "Wednesday (07:15 - 11:25)",
                    DayPattern = "Wed", SlotGroup = 1,
                    Classroom = "SE08102", Semester = "Fall 2026", Status = CourseStatus.Active }
            };

            foreach (var c in courses) uow.Courses.Add(c);

            // ─── ENROLLMENTS WITH SEEDED BTEC GRADES AND ATTENDANCE ────────
            // ─── ENROLLMENTS WITH SEEDED BTEC GRADES AND ATTENDANCE ────────
            var enrollments = new List<Enrollment>();

            var r = new Random(42);

            // Enroll special student "minh" in all courses up to term 5
            string minhId = "stu-000";
            var minhCoursesCompleted = new[] { "crs-7388", "crs-7393", "crs-7407", "crs-7398", "crs-7400", "crs-7406", "crs-7430", "crs-7481", "crs-7428", "crs-7408", "crs-7419" };
            var minhCoursesEnrolled = new[] { "crs-4902", "crs-7436", "crs-7429" };
            
            foreach (var cId in minhCoursesCompleted) {
                enrollments.Add(new Enrollment { EnrollmentId = $"enr-{minhId}-{cId}", StudentId = minhId, CourseId = cId, EnrollDate = new DateTime(2025, 1, 5), Grade = Math.Round(r.NextDouble() * 5 + 5, 1), LetterGrade = "D", Absences = r.Next(0,5), AttendancePattern = "PPPPPPPPPPPPPPPPPPPPPPPPPPPPPP", Status = EnrollmentStatus.Completed });
            }
            foreach (var cId in minhCoursesEnrolled) {
                enrollments.Add(new Enrollment { EnrollmentId = $"enr-{minhId}-{cId}", StudentId = minhId, CourseId = cId, EnrollDate = new DateTime(2026, 5, 5), Grade = null, LetterGrade = null, Absences = 0, AttendancePattern = "PPPPPPPPPPPP__________________", Status = EnrollmentStatus.Enrolled });
            }

            // Term 5: stu-001
            foreach (var studentId in new[] { "stu-001" }) {
                foreach (var cId in new[] { "crs-4902", "crs-7436", "crs-7429" }) {
                    enrollments.Add(new Enrollment { EnrollmentId = $"enr-{studentId}-{cId}", StudentId = studentId, CourseId = cId, EnrollDate = new DateTime(2026, 5, 5), Grade = null, LetterGrade = null, Absences = r.Next(0, 3), AttendancePattern = "PPPPPPPPPPPP__________________", Status = EnrollmentStatus.Enrolled, TotalSessions = 30 });
                }
            }

            foreach (var e in enrollments) uow.Enrollments.Add(e);

            // REMOVED EXTRA SEED DATA TO PREVENT FK ERRORS───────────────────────────
            // Find first 3 courses from the seeded courses to create exams for
            var courseList = uow.Courses.GetAll().Take(3).ToList();
            for (int i = 0; i < courseList.Count; i++)
            {
                uow.Exams.Add(new Exam
                {
                    ExamId = $"exam-{i+1:D3}",
                    CourseId = courseList[i].CourseId,
                    ExamType = "Final",
                    ExamDate = new DateTime(2025, 5, 15 + i),
                    TimeSlot = i % 2 == 0 ? "09:00-11:00" : "14:00-16:00",
                    Room = $"Room A{i+1}0{i+1}",
                    SupervisorId = "fac-001",
                    SupervisorName = "Dr. Nguyen Van An",
                    Semester = "2025-1",
                    Status = ExamStatus.Scheduled
                });
            }

            uow.SaveChanges();

            RecalculateAcademicAverages(uow);
        }

        /// <summary>
        /// Derives each seeded student's academic average from the marks that were just created.
        /// <para>
        /// Without this pass every demo student displayed an average of 0.00 despite having graded
        /// enrollments, because the average is normally only recalculated when a mark is entered
        /// through the application — an event that never happens for seeded data.
        /// </para>
        /// </summary>
        private static void RecalculateAcademicAverages(IUnitOfWork uow)
        {
            // Grouped once up front rather than queried per student: with a large seed set a
            // per-student query would be an N+1 round-trip pattern.
            var marksByStudent = uow.Enrollments.GetAll()
                .Where(e => e.Grade.HasValue)
                .GroupBy(e => e.StudentId)
                .ToDictionary(g => g.Key, g => Math.Round(g.Average(e => e.Grade!.Value), 2));

            foreach (var (studentId, average) in marksByStudent)
            {
                // GetById resolves through the change tracker. Re-reading with GetAll() would return
                // detached copies, and calling Update on those throws because an instance with the
                // same key is already tracked from the seeding above.
                var student = uow.Students.GetById(studentId);
                if (student is null) continue;

                student.GPA = average;
            }

            uow.SaveChanges();
        }
    }
}
