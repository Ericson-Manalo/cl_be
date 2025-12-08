using System.Security.Cryptography;
using System.Text;

namespace cl_be.Helpers
{
    public static class PasswordHelper
    {
        private static readonly Random _rng = new Random();

        // Genera 10 caratteri alfanumerici per il salt
        public static string GenerateSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[_rng.Next(s.Length)]).ToArray());
        }

        // Genera SHA-256 hash come string hex
        public static string GenerateHash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = password + salt;
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return BitConverter.ToString(hashBytes).Replace("-", ""); // hex string
        }

        // Verifica se il password inserito corriponde al hash e salt salvati nel DB
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var computedHash = GenerateHash(password, storedSalt);
            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
