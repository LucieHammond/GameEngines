using System;
using System.Diagnostics;

namespace GameEngine.Core.Logger.Base
{
    /// <summary>
    /// A predefined logger that logs messages to the trace listeners of the System.Diagnostics.Debug.Listeners collection
    /// </summary>
    public class DebugLogger : ILogger
    {
        /// <summary>
        /// <see cref="ILogger.LogDebug(string, string)"/>
        /// </summary>
        public void LogDebug(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "DEBUG");
        }

        /// <summary>
        /// <see cref="ILogger.LogInfo(string, string)"/>
        /// </summary>
        public void LogInfo(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "INFO");
        }

        /// <summary>
        /// <see cref="ILogger.LogWarning(string, string)"/>
        /// </summary>
        public void LogWarning(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "WARNING");
        }

        /// <summary>
        /// <see cref="ILogger.LogError(string, string)"/>
        /// </summary>
        public void LogError(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "ERROR");
        }

        /// <summary>
        /// <see cref="ILogger.LogException(string, Exception)"/>
        /// </summary>
        public void LogException(string tag, Exception e)
        {
            string message = LogUtils.FormatException(e);
            LogError(tag, message);
        }

        private string FormatMessage(string tag, string message)
        {
            return $"[{tag}] {message}";
        }
    }
}
