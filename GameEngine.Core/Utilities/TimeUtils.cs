using System;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for time operations
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// Unix epoch (1st January 1970, 00:00:00 UTC), from which Unix timestamps are measured
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Time zero (1st January 0001 at 00:00:00 UTC), from which DateTime ticks are measured
        /// </summary>
        public static readonly DateTime ZeroTime = new DateTime(0001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Convert a given DateTime instance to its corresponding Unix timestamp
        /// </summary>
        /// <param name="dateTime">The DateTime instance to convert</param>
        /// <returns>The corresponding Unix timestamp</returns>
        public static double ToTimestamp(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Convert a given Unix timestamp to a corresponding DateTime instance
        /// </summary>
        /// <param name="timestamp">The Unix timestamp to convert</param>
        /// <returns>The corresponding DateTime instance created</returns>
        public static DateTime ToDateTime(double timestamp)
        {
            return UnixEpoch.AddSeconds(timestamp);
        }

        /// <summary>
        /// Get the Unix timestamp of the current UTC date and time on this computer
        /// </summary>
        /// <returns>The current Unix timestamp</returns>
        public static double CurrentTimestamp()
        {
            return DateTime.UtcNow.ToTimestamp();
        }

        /// <summary>
        /// Get a string representation of a time interval (in seconds) using a specified format
        /// </summary>
        /// <param name="seconds">The time interval expressed in seconds</param>
        /// <param name="format">The format to use (a standard or custom TimeSpan format string)</param>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information</param>
        /// <returns>The formatted string representing the time interval</returns>
        public static string FormatTimeSpan(double seconds, string format, IFormatProvider cultureInfo = null)
        {
            return TimeSpan.FromSeconds(seconds).ToString(format, cultureInfo);
        }

        /// <summary>
        /// Get a string representation of a datetime (as timestamp) using a specified format
        /// </summary>
        /// <param name="timestamp">The datetime expressed as timestamp</param>
        /// <param name="format">The format to use (a standard or custom DateTime format string)</param>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information</param>
        /// <returns>The formatted string representing the date and time</returns>
        public static string FormatDateTime(double timestamp, string format, IFormatProvider cultureInfo = null)
        {
            return ToDateTime(timestamp).ToString(format, cultureInfo);
        }
    }
}
