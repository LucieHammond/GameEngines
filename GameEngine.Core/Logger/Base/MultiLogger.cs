using System;
using System.Collections.Generic;

namespace GameEngine.Core.Logger.Base
{
    public class MultiLogger : ILogger
    {
        private readonly List<ILogger> m_Loggers;

        public MultiLogger(List<ILogger> loggers)
        {
            m_Loggers = loggers;
        }

        public void LogDebug(string tag, string message)
        {
            foreach (ILogger logger in m_Loggers)
            {
                logger.LogDebug(tag, message);
            }
        }

        public void LogInfo(string tag, string message)
        {
            foreach (ILogger logger in m_Loggers)
            {
                logger.LogInfo(tag, message);
            }
        }

        public void LogWarning(string tag, string message)
        {
            foreach (ILogger logger in m_Loggers)
            {
                logger.LogWarning(tag, message);
            }
        }

        public void LogError(string tag, string message)
        {
            foreach (ILogger logger in m_Loggers)
            {
                logger.LogError(tag, message);
            }
        }

        public void LogException(string tag, Exception e)
        {
            foreach (ILogger logger in m_Loggers)
            {
                logger.LogException(tag, e);
            }
        }
    }
}
