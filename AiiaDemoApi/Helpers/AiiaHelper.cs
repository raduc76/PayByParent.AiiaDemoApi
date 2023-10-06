using System.Security.Cryptography;
using System.Text;

namespace AiiaDemoApi.Helpers
{
    public static class AiiaHelper
    {
        public static bool VerifySignature(string timestamp, string eventId, string eventType, string aiiaSignature, string payload, string webhookSecret)
        {
            if (string.IsNullOrWhiteSpace(aiiaSignature))
                return true;

            if (string.IsNullOrWhiteSpace(webhookSecret))
                return true;

            var generatedSignature = GenerateSignature(timestamp, eventId, eventType, payload, webhookSecret);

            if (generatedSignature != aiiaSignature)
            {
                return false;
            }

            return true;
        }

        static string GenerateSignature(string timestamp, string eventId, string eventType, string body, string secret)
        {
            var textBytes = Encoding.UTF8.GetBytes($"{timestamp}|{eventId}|{eventType}|{body}");
            var keyBytes = Encoding.UTF8.GetBytes(secret);

            byte[] hashBytes;
            using (var hasher = new HMACSHA256(keyBytes))
            {
                hashBytes = hasher.ComputeHash(textBytes);
            }
            // Convert hash from "AA-BB-CC-..." to "aabbcc..."
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
