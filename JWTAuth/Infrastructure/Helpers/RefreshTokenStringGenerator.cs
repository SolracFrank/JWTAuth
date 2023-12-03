using System.Security.Cryptography;

namespace Infrastructure.Helpers
{
    public static class RefreshTokenStringGenerator
    {
        public static string RandomTokenString()
        {
            using var rngCryptoServicesProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServicesProvider.GetBytes(randomBytes);

            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
    }
}
