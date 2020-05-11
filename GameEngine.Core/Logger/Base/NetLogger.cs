using System;
using System.Net.Http;
using System.Text;

namespace GameEngine.Core.Logger.Base
{
    public class NetLogger : ILogger
    {
        private Uri m_LogUrl;
        private readonly string m_AppVersion;
        private readonly string m_Environment;

        public NetLogger(Uri url, string appVersion = "1.0.0", string environment = "Local")
        {
            m_LogUrl = url;
            m_AppVersion = appVersion;
            m_Environment = environment;
        }

        public void LogDebug(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Debug, tag, message));
        }

        public void LogInfo(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Info, tag, message));
        }

        public void LogWarning(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Warning, tag, message));
        }

        public void LogError(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Error, tag, message));
        }

        public void LogException(string tag, Exception e)
        {
            string message = LogUtils.FormatException(e);
            SendJsonMessage(FormatJsonMessage(LogLevel.Error, tag, message));
        }

        public void SendJsonMessage(string jsonMessage)
        {
            using (var client = new HttpClient())
            {
                client.PostAsync(m_LogUrl,
                    new StringContent(jsonMessage, Encoding.UTF8, "application/json")).Wait();
            }
        }

        private string FormatJsonMessage(LogLevel level, string tag, string message)
        {
            return $"{{" +
                $"'appVersion':'{m_AppVersion}', " +
                $"'environment':'{m_Environment}', " +
                $"'platform':'{Environment.OSVersion.Platform}', " +
                $"'osVersion':'{Environment.OSVersion.Version}', " +
                $"'machine':'{Environment.MachineName}', " +
                $"'time':{DateTime.Now.ToUniversalTime().Ticks}, " +
                $"'level':'{level}', " +
                $"'tag':'{tag}', " +
                $"'message':'{message}'" +
                $"}}";
        }
    }
}
