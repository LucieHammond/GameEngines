using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameEngine.Core.Logger
{
    public class UnityLogger : ILogger
    {
        private Dictionary<string, Color> m_TagsColors;

        public UnityLogger(Dictionary<string, Color> tagsColors = null)
        {
            m_TagsColors = tagsColors ?? new Dictionary<string, Color>();
        }

        public void LogDebug(string tag, string message)
        {
            Debug.Log(FormatMessage(tag, message));
        }

        public void LogInfo(string tag, string message)
        {
            Debug.Log(FormatMessage(tag, message));
        }

        public void LogWarning(string tag, string message)
        {
            Debug.LogWarning(FormatMessage(tag, message));
        }

        public void LogError(string tag, string message)
        {
            Debug.LogError(FormatMessage(tag, message));
        }

        public void LogException(string tag, Exception e)
        {
            Exception firstException = e;
            while (firstException.InnerException != null)
                firstException = firstException.InnerException;

            FieldInfo messageField = firstException.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            messageField.SetValue(firstException, FormatMessage(tag, firstException.Message));
            
            Debug.LogException(e);
        }

        private string FormatMessage(string tag, string message)
        {
            Color color = GetOrAddColor(tag);
            tag = string.Format("<color=#{0:X2}{1:X2}{2:X2}><b>{3}</b></color>", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f), tag);
            return $"[{tag}] {message}";
        }

        private Color GetOrAddColor(string tag)
        {
            if (!m_TagsColors.TryGetValue(tag, out Color color))
            {
                color = LogSettings.GetColorForTag(tag);
                m_TagsColors.Add(tag, color);
            }

            return color;
        }
    }
}

