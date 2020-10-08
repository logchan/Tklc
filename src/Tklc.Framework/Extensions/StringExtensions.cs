using System.Text;

namespace Tklc.Framework.Extensions {
    public static class StringExtensions {
        /// <summary>
        /// Get the lowercase hexadecimal of a byte array.
        /// </summary>
        public static string ToHexString(this byte[] bytes) {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }
    }
}
