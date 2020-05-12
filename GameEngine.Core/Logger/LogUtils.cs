using System;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// An utility class regrouping useful methods for logs
    /// </summary>
    public static class LogUtils
    {
        /// <summary>
        /// Create a log text describing a given exception in a clear and detailed way (type, message, stacktrace, inner exceptions...)
        /// </summary>
        /// <param name="e">The exception to log</param>
        /// <returns>A formatted message fully describing the exception</returns>
        public static string FormatException(Exception e)
        {
            string logMessage = "";
            Exception exception = e;

            do
            {
                logMessage += $"{exception.GetType().Name} : {exception.Message}";

                if (!string.IsNullOrEmpty(exception.StackTrace))
                    logMessage += "\n" + exception.StackTrace;

                if (exception.InnerException != null)
                    logMessage += "\n   - InnerException -\n   >> ";

                exception = exception.InnerException;
            }
            while (exception != null);

            return logMessage;
        }

        /// <summary>
        /// Get the current time formatted like dd/MM/yyyy HH:mm:ss.ff (ready to be logged)
        /// </summary>
        /// <returns>The formatted current time</returns>
        public static string GetTime()
        {
            return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ff");
        }
    }
}
