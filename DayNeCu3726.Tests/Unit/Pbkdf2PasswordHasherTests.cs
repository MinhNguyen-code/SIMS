using System.Security.Cryptography;
using System.Text;
using DayNeCu3726.Security;

namespace DayNeCu3726.Tests.Unit
{
    /// <summary>
    /// Unit tests for the PBKDF2 password hasher.
    /// <para>
    /// Security behaviour is exactly the kind of logic that must be covered by automated tests:
    /// a regression here would be invisible in the UI yet would compromise every stored credential.
    /// </para>
    /// </summary>
    public class Pbkdf2PasswordHasherTests
    {
        // A deliberately low work factor keeps the tests fast; production uses the 120,000 default.
        private readonly Pbkdf2PasswordHasher _hasher = new(iterations: 1_000);

        [Fact]
        public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
        {
            var hash = _hasher.Hash("Student@123");

            Assert.True(_hasher.Verify("Student@123", hash));
        }

        [Fact]
        public void Verify_WithWrongPassword_ReturnsFalse()
        {
            var hash = _hasher.Hash("Student@123");

            Assert.False(_hasher.Verify("Student@124", hash));
        }

        [Fact]
        public void Verify_IsCaseSensitive()
        {
            var hash = _hasher.Hash("Student@123");

            Assert.False(_hasher.Verify("student@123", hash));
        }

        /// <summary>
        /// The critical property the old implementation lacked: because each hash uses a fresh random
        /// salt, two users with the same password get different stored values, so one rainbow table
        /// cannot crack them both.
        /// </summary>
        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            var first = _hasher.Hash("SamePassword1!");
            var second = _hasher.Hash("SamePassword1!");

            Assert.NotEqual(first, second);
            Assert.True(_hasher.Verify("SamePassword1!", first));
            Assert.True(_hasher.Verify("SamePassword1!", second));
        }

        [Fact]
        public void Hash_EmbedsAlgorithmAndIterationCount()
        {
            var hash = _hasher.Hash("Any@123");
            var segments = hash.Split('$');

            Assert.Equal(4, segments.Length);
            Assert.Equal("PBKDF2", segments[0]);
            Assert.Equal("1000", segments[1]);
        }

        [Fact]
        public void Verify_HashProducedWithDifferentIterationCount_StillSucceeds()
        {
            // Proves the work factor can be raised later without invalidating existing hashes.
            var oldHash = new Pbkdf2PasswordHasher(iterations: 1_000).Hash("Secret@1");
            var newHasher = new Pbkdf2PasswordHasher(iterations: 5_000);

            Assert.True(newHasher.Verify("Secret@1", oldHash));
        }

        [Fact]
        public void Verify_LegacySha256Hash_StillSucceeds()
        {
            var legacyHash = CreateLegacySha256Hash("Admin@123");

            Assert.True(_hasher.Verify("Admin@123", legacyHash));
        }

        [Fact]
        public void NeedsUpgrade_LegacyHash_ReturnsTrue()
        {
            var legacyHash = CreateLegacySha256Hash("Admin@123");

            Assert.True(_hasher.NeedsUpgrade(legacyHash));
        }

        [Fact]
        public void NeedsUpgrade_Pbkdf2Hash_ReturnsFalse()
        {
            var hash = _hasher.Hash("Admin@123");

            Assert.False(_hasher.NeedsUpgrade(hash));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-valid-hash")]
        [InlineData("PBKDF2$notanumber$c2FsdA==$a2V5")]
        [InlineData("PBKDF2$1000$!!!invalid-base64!!!$a2V5")]
        public void Verify_MalformedStoredHash_ReturnsFalseInsteadOfThrowing(string storedHash)
        {
            // Failing closed matters: a corrupted hash must deny access, never crash the login page.
            Assert.False(_hasher.Verify("Anything@1", storedHash));
        }

        [Fact]
        public void Verify_NullOrEmptyPassword_ReturnsFalse()
        {
            var hash = _hasher.Hash("Student@123");

            Assert.False(_hasher.Verify(string.Empty, hash));
        }

        [Fact]
        public void Constructor_WithTooFewIterations_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHasher(iterations: 10));
        }

        private static string CreateLegacySha256Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "SIMS_SALT_2024"));
            return Convert.ToBase64String(bytes);
        }
    }
}
