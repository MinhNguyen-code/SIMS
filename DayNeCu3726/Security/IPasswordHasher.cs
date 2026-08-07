namespace DayNeCu3726.Security
{
    /// <summary>
    /// Abstraction for password hashing and verification.
    /// <para>
    /// Dependency Inversion Principle (DIP): high-level policy classes such as
    /// <c>AuthService</c> and <c>StudentService</c> depend on this abstraction rather than on a
    /// concrete cryptographic implementation. The hashing algorithm can therefore be upgraded
    /// (SHA-256 -> PBKDF2 -> Argon2) without editing a single consumer.
    /// </para>
    /// <para>
    /// Interface Segregation Principle (ISP): the contract exposes only the two operations that
    /// callers actually need, nothing more.
    /// </para>
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Produces a self-describing hash string that embeds the algorithm parameters and the salt.</summary>
        string Hash(string password);

        /// <summary>Verifies a plain-text password against a stored hash in constant time.</summary>
        bool Verify(string password, string storedHash);

        /// <summary>
        /// Indicates that <paramref name="storedHash"/> was produced by a weaker legacy algorithm and
        /// should be re-hashed the next time the user successfully authenticates.
        /// </summary>
        bool NeedsUpgrade(string storedHash);
    }
}
