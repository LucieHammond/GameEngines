using System;

namespace GameEngine.Core.Logger.Base
{
    public abstract class BaseLogger : ILogger
    {
        public abstract void LogDebug(string tag, string message);

        public abstract void LogInfo(string tag, string message);

        public abstract void LogWarning(string tag, string message);

        public abstract void LogError(string tag, string message);

        public void LogException(string tag, Exception e)
        {
            string message = "";
            Exception exception = e;

            do
            {
                message += $"{exception.GetType().Name} : {exception.Message}";

                if (!string.IsNullOrEmpty(exception.StackTrace))
                    message += "\n" + exception.StackTrace;

                if (exception.InnerException != null)
                    message += "\n   - InnerException -\n   >> ";

                exception = exception.InnerException;
            }
            while (exception != null);

            LogError(tag, message);
        }

        protected string GetCurrentTime()
        {
            return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ff");
        }
    }
}
