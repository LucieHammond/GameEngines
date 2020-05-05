using System;

namespace GameEngine.Core.Logger.Base
{
    /// <summary>
    /// A predefined logger that logs messages in the console
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// <see cref="ILogger.LogDebug(string, string)"/>
        /// </summary>
        public void LogDebug(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Debug, message);
        }

        /// <summary>
        /// <see cref="ILogger.LogInfo(string, string)"/>
        /// </summary>
        public void LogInfo(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Info, message);
        }

        /// <summary>
        /// <see cref="ILogger.LogWarning(string, string)"/>
        /// </summary>
        public void LogWarning(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Warning, message);
        }

        /// <summary>
        /// <see cref="ILogger.LogError(string, string)"/>
        /// </summary>
        public void LogError(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Error, message);
        }

        /// <summary>
        /// <see cref="ILogger.LogException(string, Exception)"/>
        /// </summary>
        public void LogException(string tag, Exception e)
        {
            string message = LogUtils.FormatException(e);
            LogError(tag, message);
        }

        private void PrintMessage(string tag, LogLevel level, string message)
        {
            // Print time
            Console.Write("{0}\t", LogUtils.GetLogTime());

            // Print log level
            Console.ForegroundColor = GetLevelColor(level);
            Console.Write("{0}\t ", level.ToString().ToUpper());
            Console.ResetColor();

            // Print tag
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{tag}");
            Console.ResetColor();
            Console.Write("] ");

            // Print message
            Console.Write("{0}\n", message);
        }

        private ConsoleColor GetLevelColor(LogLevel level)
        {
            switch(level)
            {
                case LogLevel.Error:
                    return ConsoleColor.DarkRed;
                case LogLevel.Warning:
                    return ConsoleColor.DarkYellow;
                case LogLevel.Info:
                    return ConsoleColor.DarkCyan;
                case LogLevel.Debug:
                default:
                    return ConsoleColor.DarkGreen;
            }
        }
    }
}
