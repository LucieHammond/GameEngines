using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Core.Logs
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
