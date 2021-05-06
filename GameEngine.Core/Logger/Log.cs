#define ENABLE_LOGS
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// A static class accessible from anywhere, used for logging messages (with tags) using one or multiple specified loggers
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The log targets chosen to receive and display messages
        /// </summary>
        public static List<LogTarget> Targets = new List<LogTarget>();

        /// <summary>
        /// Add a new logger to the list of log targets (with associated log filtering policies) 
        /// </summary>
        /// <param name="logger">The new targetted logger</param>
        /// <param name="minLevel">The minimal level of logs to display with the targetted logger</param>
        /// <param name="tagsFilter">The tags filtering policy to apply with the targetted logger</param>
        public static void AddLogger(ILogger logger, LogLevel minLevel = LogLevel.Debug, HashSet<string> tagsFilter = null)
        {
            Targets.Add(new LogTarget(logger, minLevel, tagsFilter));
        }

        /// <summary>
        /// Log message with LogLevel = Debug
        /// </summary>
        /// <param name="tag">The tag of the message (cannot be null)</param>
        /// <param name="message">The message to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Debug(string tag, string message)
        {
            CheckTagValidity(tag);

            foreach (LogTarget target in Targets)
            {
                if (target.MinLevel <= LogLevel.Debug)
                {
                    if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                        target.Logger.LogDebug(tag, message);
                }
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

            foreach (LogTarget target in Targets)
            {
                if (target.MinLevel <= LogLevel.Info)
                {
                    if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                        target.Logger.LogInfo(tag, message);
                }
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

            foreach (LogTarget target in Targets)
            {
                if (target.MinLevel <= LogLevel.Warning)
                {
                    if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                        target.Logger.LogWarning(tag, message);
                }
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

            foreach (LogTarget target in Targets)
            {
                if (target.MinLevel <= LogLevel.Error)
                {
                    if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                        target.Logger.LogError(tag, message);
                }
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
        /// <param name="exception">The exception to log</param>
        [Conditional("ENABLE_LOGS")]
        public static void Exception(string tag, Exception exception)
        {
            CheckTagValidity(tag);

            foreach (LogTarget target in Targets)
            {
                if (target.MinLevel <= LogLevel.Error)
                {
                    if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                        target.Logger.LogException(tag, exception);
                }
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

            foreach (LogTarget target in Targets)
            {
                if (target.TagsFilter == null || target.TagsFilter.Contains(tag))
                    target.Logger.LogError(tag, message);
            }
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
