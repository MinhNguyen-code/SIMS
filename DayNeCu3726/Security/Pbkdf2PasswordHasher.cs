using System.Security.Cryptography;
using System.Text;

namespace DayNeCu3726.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA256 password hasher with a cryptographically random per-user salt.
    /// <para>
    /// Replaces the previous <c>SHA256(password + "SIMS_SALT_2024")</c> scheme, which was unsafe
    /// because (a) a single shared salt lets an attacker build one rainbow table for every account
    /// and (b) raw SHA-256 is fast, so brute-forcing it is cheap.
    /// </para>
    /// <para>
    /// Stored format: <c>PBKDF2$&lt;iterations&gt;$&lt;base64 salt&gt;$&lt;base64 subkey&gt;</c>.
    /// Embedding the parameters keeps old hashes verifiable after the work factor is raised.
    /// </para>
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const char Separator = '$';
        private const int SaltSizeInBytes = 16;
        private const int SubkeySizeInBytes = 32;
        private const int DefaultIterations = 120_000;

        /// <summary>Salt used by the superseded SHA-256 scheme, kept only so legacy logins still work.</summary>
        private const string LegacySalt = "SIMS_SALT_2024";

        private readonly int _iterations;

        public Pbkdf2PasswordHasher(int iterations = DefaultIterations)
        {
            if (iterations < 1_000)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iteration count is too low to be secure.");

            _iterations = iterations;
        }

        public string Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSizeInBytes);
            var subkey = DeriveSubkey(password, salt, _iterations);

            return string.Join(Separator,
                Prefix,
                _iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(subkey));
        }

        public bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            return IsLegacyHash(storedHash)
                ? VerifyLegacy(password, storedHash)
                : VerifyPbkdf2(password, storedHash);
        }

        public bool NeedsUpgrade(string storedHash) => IsLegacyHash(storedHash);

        private static bool IsLegacyHash(string storedHash) =>
            !storedHash.StartsWith(Prefix + Separator, StringComparison.Ordinal);

        private bool VerifyPbkdf2(string password, string storedHash)
        {
            var segments = storedHash.Split(Separator);
            if (segments.Length != 4 || !int.TryParse(segments[1], out var iterations) || iterations <= 0)
                return false;

            byte[] salt, expectedSubkey;
            try
            {
                salt = Convert.FromBase64String(segments[2]);
                expectedSubkey = Convert.FromBase64String(segments[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualSubkey = DeriveSubkey(password, salt, iterations, expectedSubkey.Length);

            // Fixed-time comparison prevents timing side-channel attacks.
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }

        /// <summary>
        /// Verifies a hash created by the original SHA-256 implementation so that accounts seeded
        /// before this upgrade can still sign in. <see cref="NeedsUpgrade"/> then tells the caller
        /// to re-hash the password with PBKDF2.
        /// </summary>
        private static bool VerifyLegacy(string password, string storedHash)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + LegacySalt));
            var legacyHash = Convert.ToBase64String(bytes);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacyHash),
                Encoding.UTF8.GetBytes(storedHash));
        }

        private static byte[] DeriveSubkey(string password, byte[] salt, int iterations, int length = SubkeySizeInBytes) =>
            Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                length);
    }
}
