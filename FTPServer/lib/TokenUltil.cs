using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public static class TokenUltil
    {
        public static string GenerateToken(string data, string secretKey)
        {
            string signature = ComputeHmacSha256(data, secretKey);
            return $"{data}.{signature}";
        }

        public static bool VerifyToken(string token, string secretKey)
        {
            if (String.IsNullOrEmpty(token)) return false;
            string[] parts = token.Split('.');
            if (parts.Length != 2) return false;

            string userId = parts[0];
            string expectedSignature = ComputeHmacSha256(userId, secretKey);

            return expectedSignature == parts[1];
        }

        private static string ComputeHmacSha256(string data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(dataBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
