#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// A static class accessible from anywhere, used for logging messages (with tags) using a predefined logger
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The logger to use for logging messages
        /// </summary>
        public static ILogger Logger;

        /// <summary>
        /// The minimal level of logs to display. All logs with an inferior level of importance will be ignored
        /// </summary>
        public static LogLevel MinLevel = LogLevel.Debug;

        /// <summary>
        /// The tags on which to filter the logs. If null, all logs are displayed. Otherwise, only the logs having the right tags are displayed
        /// </summary>
        public static HashSet<string> TagsFilter;

        /// <summary>
        /// Log message with LogLevel = Debug
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Debug(string tag, string message)
        {
            CheckTagValidity(tag);

            if (MinLevel <= LogLevel.Debug)
            {
                if (TagsFilter == null || TagsFilter.Contains(tag))
                    Logger?.LogDebug(tag, message);
            }
        }

        /// <summary>
        /// Log formatted message with LogLevel = Debug
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="format">The formatted string representing the message</param>
        /// <param name="args">The arguments to use as replacement in the format</param>
        [Conditional("ENABLE_LOGS")]
        public static void Debug(string tag, string format, params object[] args)
        {
            Debug(tag, string.Format(format, args));
        }

        /// <summary>
        /// Log tagged message with LogLevel = Info
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Info(string tag, string message)
        {
            CheckTagValidity(tag);

            if (MinLevel <= LogLevel.Info)
            {
                if (TagsFilter == null || TagsFilter.Contains(tag))
                    Logger?.LogInfo(tag, message);
            }
        }

        /// <summary>
        /// Log tagged message with LogLevel = Info
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="format">The formatted string representing the message</param>
        /// <param name="args">The arguments to use as replacement in the format</param>
        [Conditional("ENABLE_LOGS")]
        public static void Info(string tag, string format, params object[] args)
        {
            Info(tag, string.Format(format, args));
        }

        /// <summary>
        /// Log tagged message with LogLevel = Warning
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Warning(string tag, string message)
        {
            CheckTagValidity(tag);

            if (MinLevel <= LogLevel.Warning)
            {
                if (TagsFilter == null || TagsFilter.Contains(tag))
                    Logger?.LogWarning(tag, message);
            }
        }

        /// <summary>
        /// Log tagged message with LogLevel = Warning
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="format">The formatted string representing the message</param>
        /// <param name="args">The arguments to use as replacement in the format</param>
        [Conditional("ENABLE_LOGS")]
        public static void Warning(string tag, string format, params object[] args)
        {
            Warning(tag, string.Format(format, args));
        }

        /// <summary>
        /// Log tagged message with LogLevel = Error
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Error(string tag, string message)
        {
            CheckTagValidity(tag);

            if (MinLevel <= LogLevel.Error)
            {
                if (TagsFilter == null || TagsFilter.Contains(tag))
                    Logger?.LogError(tag, message);
            }
        }

        /// <summary>
        /// Log tagged message with LogLevel = Error
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="format">The formatted string representing the message</param>
        /// <param name="args">The arguments to use as replacement in the format</param>
        [Conditional("ENABLE_LOGS")]
        public static void Error(string tag, string format, params object[] args)
        {
            Error(tag, string.Format(format, args));
        }

        /// <summary>
        /// Log tagged exception with LogLevel = Error
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="e">The exception to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Exception(string tag, Exception e)
        {
            CheckTagValidity(tag);

            if (MinLevel <= LogLevel.Error)
            {
                if (TagsFilter == null || TagsFilter.Contains(tag))
                    Logger?.LogException(tag, e);
            }
        }

        /// <summary>
        /// Log tagged message with LogLevel = Fatal
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Fatal(string tag, string message)
        {
            CheckTagValidity(tag);

            if (TagsFilter == null || TagsFilter.Contains(tag))
                Logger?.LogError(tag, message);
        }

        /// <summary>
        /// Log tagged message with LogLevel = Fatal
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="format">The formatted string representing the message</param>
        /// <param name="args">The arguments to use as replacement in the format</param>
        [Conditional("ENABLE_LOGS")]
        public static void Fatal(string tag, string format, params object[] args)
        {
            Fatal(tag, string.Format(format, args));
        }

        private static void CheckTagValidity(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException("Logging tag cannot be null or empty");
        }
    }
}
