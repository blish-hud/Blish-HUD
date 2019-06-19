using System;
using System.Text;

namespace Blish_HUD {
    public static class StringExtensions {
        
        private static readonly int _charSize = sizeof(char);

        public static unsafe byte[] GetBytes(this string str) {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (str.Length == 0) return new byte[0];

            fixed (char* p = str) {
                return new Span<byte>(p, str.Length * _charSize).ToArray();
            }
        }

        public static unsafe string GetString(this byte[] bytes) {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length % _charSize != 0) throw new ArgumentException($"Invalid {nameof(bytes)} length");
            if (bytes.Length == 0) return string.Empty;

            fixed (byte* p = bytes) {
                return new string(new Span<char>(p, bytes.Length / _charSize).ToArray());
            }
        }

        // From https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?redirectedfrom=MSDN&view=netframework-4.7.2
        public static string GetMD5Hash(this string str) {
            // Use input string to calculate MD5 hash
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                //byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(str);
                byte[] inputBytes = str.GetBytes();
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

    }
}
