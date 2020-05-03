using System;
using System.Collections.Generic;

namespace GameEngine.Core.Logger.Base
{
    public class ConsoleLogger : BaseLogger
    {
        public override void LogDebug(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Debug, message);
        }

        public override void LogInfo(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Info, message);
        }

        public override void LogWarning(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Warning, message);
        }

        public override void LogError(string tag, string message)
        {
            PrintMessage(tag, LogLevel.Error, message);
        }

        private void PrintMessage(string tag, LogLevel level, string message)
        {
            // Print time
            Console.Write("{0}\t", GetCurrentTime());

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
