using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;

namespace DayNeCu3726.Repositories.InMemory
{
    /// <summary>
    /// In-memory Student repository implementation.
    /// </summary>
    public class StudentRepository : InMemoryRepository<Student>, IStudentRepository
    {
        protected override string GetId(Student entity) => entity.Id;

        public Student? GetByStudentCode(string studentCode)
            => _store.FirstOrDefault(s => s.StudentCode == studentCode);

        public Student? GetByEmail(string email)
            => _store.FirstOrDefault(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Student> GetByProgram(string program)
            => _store.Where(s => s.Program.Equals(program, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Student> GetByDepartment(string department)
            => _store.Where(s => s.Department.Equals(department, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Student> GetByEnrollmentYear(int year)
            => _store.Where(s => s.EnrollmentYear == year);

        public IEnumerable<Student> SearchByName(string name)
            => _store.Where(s => s.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Mirrors the EF implementation: derive the next code from the highest one already issued
        /// so that deletions never cause a duplicate code to be handed out.
        /// </summary>
        public string GenerateStudentCode(int year)
        {
            var highestSequence = _store
                .Select(s => s.StudentCode)
                .Select(ParseSequence)
                .DefaultIfEmpty(0)
                .Max();

            var candidateSequence = highestSequence + 1;
            var candidate = $"BH{candidateSequence:D5}";

            while (_store.Any(s => s.StudentCode == candidate))
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
