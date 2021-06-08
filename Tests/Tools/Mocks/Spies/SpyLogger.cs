using GameEngine.Core.Logger;
using System;

namespace GameEnginesTest.Tools.Mocks.Spies
{
    public class SpyLogger : ILogger
    {
        public int LogDebugCalls = 0;
        public int LogInfoCalls = 0;
        public int LogWarningCalls = 0;
        public int LogErrorCalls = 0;
        public int LogExceptionCalls = 0;

        public Action<string, string> OnLogDebug;
        public Action<string, string> OnLogInfo;
        public Action<string, string> OnLogWarning;
        public Action<string, string> OnLogError;
        public Action<string, Exception> OnLogException;

        private static SpyLogger m_DebugLogger;

        public static SpyLogger GetDebugLogger()
        {
            if (m_DebugLogger == null)
                m_DebugLogger = new SpyLogger();
            return m_DebugLogger;
        }

        public void LogDebug(string tag, string message)
        {
            LogDebugCalls++;
            OnLogDebug?.Invoke(tag, message);
        }

        public void LogInfo(string tag, string message)
        {
            LogInfoCalls++;
            OnLogInfo?.Invoke(tag, message);
        }

        public void LogWarning(string tag, string message)
        {
            LogWarningCalls++;
            OnLogWarning?.Invoke(tag, message);
        }

        public void LogError(string tag, string message)
        {
            LogErrorCalls++;
            OnLogError?.Invoke(tag, message);
        }

        public void LogException(string tag, Exception e)
        {
            LogExceptionCalls++;
            OnLogException?.Invoke(tag, e);
        }

        public void Clear()
        {
            LogDebugCalls = 0;
            LogInfoCalls = 0;
            LogWarningCalls = 0;
            LogErrorCalls = 0;
            LogExceptionCalls = 0;
        }
    }
}
