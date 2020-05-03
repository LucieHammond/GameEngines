using System;

namespace GameEngine.Core.Logger
{
    public interface ILogger
    {
        void LogDebug(string tag, string message);

        void LogInfo(string tag, string message);

        void LogWarning(string tag, string message);

        void LogError(string tag, string message);

        void LogException(string tag, Exception e);
    }
}
