using DayNeCu3726.Security;

namespace DayNeCu3726.Tests.TestDoubles
{
    /// <summary>
    /// Developer-produced test double for <see cref="IPasswordHasher"/>.
    /// <para>
    /// Hand-written stubs like this are contrasted with the vendor-provided mocking library (Moq)
    /// used elsewhere in the suite. The trade-off is deliberate: this stub is explicit, dependency
    /// free and extremely fast, which matters because the real PBKDF2 hasher performs 120,000
    /// iterations per call and would otherwise dominate the runtime of every service test.
    /// </para>
    /// </summary>
    public sealed class FakePasswordHasher : IPasswordHasher
    {
        public const string Prefix = "FAKE:";

        public int HashCallCount { get; private set; }
        public int VerifyCallCount { get; private set; }

        public string Hash(string password)
        {
            HashCallCount++;
            return Prefix + password;
        }

        public bool Verify(string password, string storedHash)
        {
            VerifyCallCount++;
            return storedHash == Prefix + password;
        }

        public bool NeedsUpgrade(string storedHash) => !storedHash.StartsWith(Prefix, StringComparison.Ordinal);
    }
}
