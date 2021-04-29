using System;
using System.Security.Cryptography;
using System.Text;

namespace Discounting.Common.Helpers
{
    public static class SecurityToolkit
    {
        private const int SaltLength = 512;

        public static string GetSha512Hash(string input)
        {
            using var hashAlgorithm = new SHA512CryptoServiceProvider();
            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        public static string GetSha512Hash(string input, string salt)
        {
            return GetSha512Hash(salt + input);
        }

        public static string GetSalt()
        {
            var salt = new byte[SaltLength];
            using var random = new RNGCryptoServiceProvider();
            random.GetNonZeroBytes(salt);

            return Convert.ToBase64String(salt);
        }
    }
}