using DayNeCu3726.DataProcessing.Abstractions;
using DayNeCu3726.DataProcessing.Mapping;
using DayNeCu3726.DataProcessing.Validation;
using DayNeCu3726.Models.Entities;
using DayNeCu3726.Repositories.Interfaces;
using DayNeCu3726.Security;

namespace DayNeCu3726.DataProcessing.Pipeline
{
    /// <summary>
    /// Concrete large-dataset importer for student records.
    /// <para>
    /// Supplies only the three entity-specific steps of <see cref="BatchImportProcessor{TEntity}"/>;
    /// all streaming, batching, error capture and timing are inherited. That is the practical payoff
    /// of the Template Method pattern — this class is short because the algorithm lives in the base.
    /// </para>
    /// </summary>
    public sealed class StudentCsvImportProcessor : BatchImportProcessor<Student>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly StudentCsvMapper _mapper;
        private readonly HashSet<string> _existingEmails;
        private readonly string _defaultPasswordHash;
        private int _nextStudentSequence;

        public StudentCsvImportProcessor(
            ICsvRecordReader reader,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher)
            : base(reader)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _mapper = new StudentCsvMapper();

            // Loaded once instead of querying per row: a per-row existence check would make the import
            // O(n) database round-trips, which is the classic N+1 problem on a large file.
            _existingEmails = _unitOfWork.Students
                .GetAll()
                .Select(s => s.Email)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            _nextStudentSequence = _unitOfWork.Students.Count() + 1;

            // Hashing PBKDF2 is deliberately slow, so the shared default password is hashed once
            // per run rather than once per row.
            _defaultPasswordHash = _passwordHasher.Hash("Student@123");
        }

        protected override IReadOnlyList<string> RequiredColumns => StudentCsvMapper.RequiredColumns;

        protected override RowValidationHandler BuildValidationChain()
        {
            var head = new RequiredColumnsHandler("FullName", "Email", "Program");

            head.SetNext(new EmailFormatHandler("Email"))
                .SetNext(new UniqueValueHandler("Email"))
                .SetNext(new DateFormatHandler("DateOfBirth"))
                .SetNext(new NumericRangeHandler("GPA", 0, 10, optional: true))
                .SetNext(new NumericRangeHandler("EnrollmentYear", 1900, 2100, optional: true));

            return head;
        }

        protected override Student? MapRecord(CsvRecord record, ImportOptions options)
        {
            var email = record["Email"].ToLowerInvariant();

            if (_existingEmails.Contains(email) && !options.UpdateExisting)
                return null;    // Already present and updates are disabled — skip rather than fail.

            var student = _mapper.ToEntity(record, _defaultPasswordHash);

            if (string.IsNullOrWhiteSpace(student.StudentCode))
                student.StudentCode = $"BH{_nextStudentSequence:D5}";

            _nextStudentSequence++;
            _existingEmails.Add(email);

            return student;
        }

        protected override Task PersistBatchAsync(IReadOnlyList<Student> batch, ImportOptions options, CancellationToken cancellationToken)
        {
            foreach (var student in batch)
            {
                _unitOfWork.Students.Add(student);
                _unitOfWork.Users.Add(student);
            }

            // One SaveChanges per batch keeps the transaction small enough to avoid lock escalation
            // while still amortising the round-trip cost across many rows.
            _unitOfWork.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
