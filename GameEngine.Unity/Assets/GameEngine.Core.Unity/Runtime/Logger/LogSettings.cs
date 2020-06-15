using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameEngine.Core.Logger
{
    public class LogSettings : ScriptableObject
    {
        public LogLevel MinLogLevel;

        public bool ActivateFiltering;

        public HashSet<string> TagsFilter;

        public Dictionary<string, Color> TagsColors;

        public LogSettings()
        {
            MinLogLevel = LogLevel.Info;
            ActivateFiltering = false;
            TagsFilter = new HashSet<string>();
            TagsColors = new Dictionary<string, Color>();
        }

        public static LogSettings GetSettings()
        {
            LogSettings settings = (LogSettings) Resources.Load("LogSettings");
            if (settings == null)
                settings = CreateInstance<LogSettings>();

            return settings;
        }

        public static Color GetColorForTag(string tag)
        {
            ColorUtility.TryParseHtmlString($"#{Convert.ToString(tag.GetHashCode(), 16)}", out Color color);
            color.r = Mathf.Lerp(0, 0.9f, color.r);
            color.g = Mathf.Lerp(0, 0.9f, color.g);
            color.b = Mathf.Lerp(0, 0.9f, color.b);
            color.a = 1;

            return color;
        }
    }
}
