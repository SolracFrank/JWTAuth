using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Helpers
{
    public static class PasswordHasher
    {
        public static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }
        public static string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                var hash = sha256.ComputeHash(hashedPassword);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
