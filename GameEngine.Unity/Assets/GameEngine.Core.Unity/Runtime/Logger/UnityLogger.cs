using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// A predefined logger that logs messages in the Unity console window
    /// </summary>
    public class UnityLogger : ILogger
    {
        private readonly ConcurrentDictionary<string, Color> m_TagsColors;

        /// <summary>
        /// Initialize a new instance of UnityLogger
        /// </summary>
        /// <param name="tagsColors">The default colors to use when displaying the associated tags</param>
        public UnityLogger(Dictionary<string, Color> tagsColors = null)
        {
            m_TagsColors = tagsColors != null ?
                new ConcurrentDictionary<string, Color>(tagsColors) :
                new ConcurrentDictionary<string, Color>();
        }

        /// <summary>
        /// <see cref="ILogger.LogDebug(string, string)"/>
        /// </summary>
        public void LogDebug(string tag, string message)
        {
            Debug.Log(FormatMessage(tag, message, true));
        }

        /// <summary>
        /// <see cref="ILogger.LogInfo(string, string)"/>
        /// </summary>
        public void LogInfo(string tag, string message)
        {
            Debug.Log(FormatMessage(tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogWarning(string, string)"/>
        /// </summary>
        public void LogWarning(string tag, string message)
        {
            Debug.LogWarning(FormatMessage(tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogError(string, string)"/>
        /// </summary>
        public void LogError(string tag, string message)
        {
            Debug.LogError(FormatMessage(tag, message));
        }

        /// <summary>
        /// <see cref="ILogger.LogException(string, Exception)"/>
        /// </summary>
        public void LogException(string tag, Exception e)
        {
            Exception firstException = e;
            while (firstException.InnerException != null)
                firstException = firstException.InnerException;

            FieldInfo messageField = firstException.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            messageField.SetValue(firstException, FormatMessage(tag, firstException.Message));

            Debug.LogException(e);
        }

        private string FormatMessage(string tag, string message, bool isDebug = false)
        {
            Color color = GetOrAddColor(tag);
            tag = string.Format("<color=#{0:X2}{1:X2}{2:X2}><b>{3}</b></color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tag);
            if (isDebug)
                message = $"<color=#FBFB99>{message}</color>";
            return $"[{tag}] {message}";
        }

        private Color GetOrAddColor(string tag)
        {
            if (!m_TagsColors.TryGetValue(tag, out Color color))
            {
                color = LogSettings.GetColorForTag(tag);
                m_TagsColors.TryAdd(tag, color);
            }

            return color;
        }
    }
}

