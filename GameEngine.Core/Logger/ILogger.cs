using System;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// The interface to implement for defining a new Logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="tag">The tag (category) associated with the message</param>
        /// <param name="message">The log message</param>
        void LogDebug(string tag, string message);

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="tag">The tag (category) associated with the message</param>
        /// <param name="message">The log message</param>
        void LogInfo(string tag, string message);

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="tag">The tag (category) associated with the message</param>
        /// <param name="message">The log message</param>
        void LogWarning(string tag, string message);

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="tag">The tag (category) associated with the message</param>
        /// <param name="message">The log message</param>
        void LogError(string tag, string message);

        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="tag">The tag (category) associated with the message</param>
        /// <param name="e">The exception to log</param>
        void LogException(string tag, Exception e);
    }
}
