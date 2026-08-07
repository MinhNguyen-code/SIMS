using Microsoft.EntityFrameworkCore;
using DayNeCu3726.Infrastructure;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.EF
{
    public class EFStudentRepository : EFRepository<Student>, IStudentRepository
    {
        public EFStudentRepository(AppDbContext context) : base(context)
        {
        }

        public Student? GetByStudentCode(string studentCode)
        {
            return _dbSet.FirstOrDefault(s => s.StudentCode == studentCode);
        }

        public Student? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(s => s.Email.ToLower() == email.ToLower());
        }

        public IEnumerable<Student> GetByProgram(string program)
        {
            return _dbSet.Where(s => s.Program.ToLower() == program.ToLower()).ToList();
        }

        public IEnumerable<Student> GetByDepartment(string department)
        {
            return _dbSet.Where(s => s.Department.ToLower() == department.ToLower()).ToList();
        }

        public IEnumerable<Student> GetByEnrollmentYear(int year)
        {
            return _dbSet.Where(s => s.EnrollmentYear == year).ToList();
        }

        public IEnumerable<Student> SearchByName(string name)
        {
            return _dbSet.Where(s => Microsoft.EntityFrameworkCore.EF.Functions.Like(s.FullName, $"%{name}%")).ToList();
        }

        /// <summary>
        /// Produces the next unused student code.
        /// <para>
        /// The previous version returned <c>"BH" + (Count() + 1)</c>. That was unsafe on two counts:
        /// deleting any student made the counter reuse an existing code, and two concurrent
        /// registrations both read the same count and generated the same code. Deriving the number
        /// from the highest code already issued and then probing for a free slot removes both faults.
        /// </para>
        /// </summary>
        public string GenerateStudentCode(int year)
        {
            var highestSequence = _dbSet
                .Select(s => s.StudentCode)
                .AsEnumerable()
                .Select(ParseSequence)
                .DefaultIfEmpty(0)
                .Max();

            var candidateSequence = highestSequence + 1;
            var candidate = $"BH{candidateSequence:D5}";

            // Guards against a code that exists with a different format or was inserted concurrently.
            while (_dbSet.Any(s => s.StudentCode == candidate))
            {
                candidateSequence++;
                candidate = $"BH{candidateSequence:D5}";
            }

            return candidate;
        }

        private static int ParseSequence(string? studentCode)
        {
            if (string.IsNullOrWhiteSpace(studentCode))
                return 0;

            var digits = new string(studentCode.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var parsed) ? parsed : 0;
        }
    }
}
