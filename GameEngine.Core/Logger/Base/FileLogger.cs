using System;
using System.IO;

namespace GameEngine.Core.Logger.Base
{
    /// <summary>
    /// A predefined logger that logs messages in a given file
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string m_LogFilePath;
        private readonly object m_FileLock = new object();

        /// <summary>
        /// FileLogger constructor
        /// </summary>
        /// <param name="logFilePath">The absolute path of the file on which to write logs</param>
        public FileLogger(string logFilePath)
        {
            if (!File.Exists(logFilePath))
                File.Create(logFilePath).Close();

            m_LogFilePath = logFilePath;
        }

        /// <summary>
        /// <see cref="ILogger.LogDebug(string, string)"/>
        /// </summary>
        public void LogDebug(string tag, string message)
        {
            WriteMessage(tag, "DEBUG", message);
        }

        /// <summary>
        /// <see cref="ILogger.LogInfo(string, string)"/>
        /// </summary>
        public void LogInfo(string tag, string message)
        {
            WriteMessage(tag, "INFO", message);
        }

        /// <summary>
        /// <see cref="ILogger.LogWarning(string, string)"/>
        /// </summary>
        public void LogWarning(string tag, string message)
        {
            WriteMessage(tag, "WARNING", message);
        }

        /// <summary>
        /// <see cref="ILogger.LogError(string, string)"/>
        /// </summary>
        public void LogError(string tag, string message)
        {
            WriteMessage(tag, "ERROR", message);
        }

        /// <summary>
        /// <see cref="ILogger.LogException(string, Exception)"/>
        /// </summary>
        public void LogException(string tag, Exception e)
        {
            string message = LogUtils.FormatException(e);
            LogError(tag, message);
        }

        private void WriteMessage(string tag, string level, string message)
        {
            lock (m_FileLock)
            {
                using StreamWriter logFileWriter = new StreamWriter(m_LogFilePath, true);
                logFileWriter.WriteLine("{0}\t{1}\t [{2}] {3}", LogUtils.GetTime(), level, tag, message);
            }
        }
    }
}
