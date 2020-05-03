using System.Diagnostics;

namespace GameEngine.Core.Logger.Base
{
    public class DebugLogger : BaseLogger
    {
        public override void LogDebug(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "DEBUG");
        }

        public override void LogInfo(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "INFO");
        }

        public override void LogWarning(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "WARNING");
        }

        public override void LogError(string tag, string message)
        {
            Debug.WriteLine(FormatMessage(tag, message), "ERROR");
        }

        private string FormatMessage(string tag, string message)
        {
            return $"[{tag}] {message}";
        }
    }
}
