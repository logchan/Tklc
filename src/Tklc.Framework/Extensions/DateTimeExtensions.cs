using System;

namespace Tklc.Framework.Extensions {
    public static class DateTimeExtensions {
        /// <summary>
        /// Get unix timestamp of a DateTime object.
        /// </summary>
        public static long ToUnixTimestamp(this DateTime dateTime, long offset = 0) {
            return (long)(
                TimeZoneInfo.ConvertTimeToUtc(dateTime).AddSeconds(offset) -
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Get unix timestamp times 1000, plus milliseconds, of a DateTime object.
        /// </summary>
        public static long ToUnixTimestampWithFractions(this DateTime dateTime, long offset = 0) {
            var seconds = dateTime.ToUnixTimestamp(offset);
            return seconds * 1000 + dateTime.Millisecond;
        }
    }
}
