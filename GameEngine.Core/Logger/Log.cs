using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.Core.Logger
{
    public static class Log
    {
        public static ILogger Logger;
        public static LogLevel MinLevel = LogLevel.Info;
        public static HashSet<string> TagsFilter;

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

        [Conditional("ENABLE_LOGS")]
        public static void Debug(string tag, string format, params object[] args)
        {
            Debug(tag, string.Format(format, args));
        }

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

        [Conditional("ENABLE_LOGS")]
        public static void Info(string tag, string format, params object[] args)
        {
            Info(tag, string.Format(format, args));
        }

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

        [Conditional("ENABLE_LOGS")]
        public static void Warning(string tag, string format, params object[] args)
        {
            Warning(tag, string.Format(format, args));
        }

        [Conditional("ENABLE_LOGS")]
        public static void Error(string tag, string message)
        {
            CheckTagValidity(tag);

            if (TagsFilter == null || TagsFilter.Contains(tag))
                Logger?.LogError(tag, message);
        }

        [Conditional("ENABLE_LOGS")]
        public static void Error(string tag, string format, params object[] args)
        {
            Error(tag, string.Format(format, args));
        }

        [Conditional("ENABLE_LOGS")]
        public static void Exception(string tag, Exception e)
        {
            CheckTagValidity(tag);

            if (TagsFilter == null || TagsFilter.Contains(tag))
                Logger?.LogException(tag, e);
        }

        private static void CheckTagValidity(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentException("Logging tag cannot be null or empty");
        }
    }
}
