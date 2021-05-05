using System;
using System.Net.Http;
using System.Text;

namespace GameEngine.Core.Logger.Base
{
    /// <summary>
    /// A predefined logger that send json formatted log messages to a remote log API
    /// </summary>
    public class NetLogger : ILogger
    {
        private Uri m_LogUrl;
        private readonly string m_AppVersion;
        private readonly string m_Environment;

        /// <summary>
        /// Initialize a new instance of NetLogger
        /// </summary>
        /// <param name="url">The URL of the remote log API</param>
        /// <param name="appVersion">The version of the application to associate with the log messages</param>
        /// <param name="environment">The running environment to associate with the log messages</param>
        public NetLogger(Uri url, string appVersion = "1.0.0", string environment = "Local")
        {
            m_LogUrl = url;
            m_AppVersion = appVersion;
            m_Environment = environment;
        }

        /// <summary>
        /// <see cref="ILogger.LogDebug(string, string)"/>
        /// </summary>
        public void LogDebug(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Debug, tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogInfo(string, string)"/>
        /// </summary>
        public void LogInfo(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Info, tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogWarning(string, string)"/>
        /// </summary>
        public void LogWarning(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Warning, tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogError(string, string)"/>
        /// </summary>
        public void LogError(string tag, string message)
        {
            SendJsonMessage(FormatJsonMessage(LogLevel.Error, tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogException(string, Exception)"/>
        /// </summary>
        public void LogException(string tag, Exception e)
        {
            string message = LogUtils.FormatException(e);
            SendJsonMessage(FormatJsonMessage(LogLevel.Error, tag, message));
        }

        private async void SendJsonMessage(string jsonMessage)
        {
            using (var client = new HttpClient())
            {
                await client.PostAsync(m_LogUrl,
                    new StringContent(jsonMessage, Encoding.UTF8, "application/json"));
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
