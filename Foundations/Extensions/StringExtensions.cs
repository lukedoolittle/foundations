using System;
using System.Text;

namespace Foundations.Extensions
{
    public static class StringExtensions
    {
        public static string FromModifiedBase64String(this string instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException();
            }
            var base64String = instance.Replace('-', '+').Replace('_', '/');
            var decodedBytes = Convert.FromBase64String(base64String);
            var decodedText = Encoding.UTF8.GetString(
                decodedBytes,
                0,
                decodedBytes.Length);
            return decodedText;
        }

        public static string ToBase64String(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
