using System.IO;

namespace GameEngine.Core.Logger.Base
{
    public class FileLogger : BaseLogger
    {
        private readonly string m_LogFilePath;

        public FileLogger(string logFilePath)
        {
            if (!File.Exists(logFilePath))
                File.Create(logFilePath).Close();

            m_LogFilePath = logFilePath;
        }

        public override void LogDebug(string tag, string message)
        {
            using StreamWriter logFileWriter = new StreamWriter(m_LogFilePath, true);
            logFileWriter.WriteLine("{0}\t{1}\t [{2}] {3}", GetCurrentTime(), "DEBUG", tag, message);
        }

        public override void LogInfo(string tag, string message)
        {
            using StreamWriter logFileWriter = new StreamWriter(m_LogFilePath, true);
            logFileWriter.WriteLine("{0}\t{1}\t [{2}] {3}", GetCurrentTime(), "INFO", tag, message);
        }

        public override void LogWarning(string tag, string message)
        {
            using StreamWriter logFileWriter = new StreamWriter(m_LogFilePath, true);
            logFileWriter.WriteLine("{0}\t{1}\t [{2}] {3}", GetCurrentTime(), "WARNING", tag, message);
        }

        public override void LogError(string tag, string message)
        {
            using StreamWriter logFileWriter = new StreamWriter(m_LogFilePath, true);
            logFileWriter.WriteLine("{0}\t{1}\t [{2}] {3}", GetCurrentTime(), "ERROR", tag, message);
        }
    }
}
