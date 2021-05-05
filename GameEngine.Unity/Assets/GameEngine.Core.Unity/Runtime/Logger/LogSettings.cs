using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// Settings for the runtime logs, allowing customization of display and filtering
    /// </summary>
    public class LogSettings : ScriptableObject
    {
        /// <summary>
        /// The name of the asset that should store the Log Settings
        /// </summary>
        public const string ASSET_NAME = "LogSettings";

        /// <summary>
        /// The minimal level of logs to display. All logs with an inferior level of importance will be ignored
        /// </summary>
        public LogLevel MinLogLevel;

        /// <summary>
        /// Whether or not to filter the logs on their tags
        /// </summary>
        public bool ActivateFiltering;

        /// <summary>
        /// The tags on which to filter the logs. Only the logs corresponding to the tags in the list are displayed
        /// </summary>
        public HashSet<string> TagsFilter;

        /// <summary>
        /// The colors to use when displaying the specific associated tags
        /// </summary>
        public Dictionary<string, Color> TagsColors;

        /// <summary>
        /// Initialize a new instance of LogSettings
        /// </summary>
        public LogSettings()
        {
            MinLogLevel = LogLevel.Info;
            ActivateFiltering = false;
            TagsFilter = new HashSet<string>();
            TagsColors = new Dictionary<string, Color>();
        }

        /// <summary>
        /// Get the log settings that have been defined in the project through a dedicated asset (or default if not defined)
        /// </summary>
        /// <returns>The requested instance of LogSettings</returns>
        public static LogSettings GetSettings()
        {
            LogSettings settings = Resources.Load<LogSettings>(ASSET_NAME);
            if (settings == null)
                settings = CreateInstance<LogSettings>();

            return settings;
        }

        /// <summary>
        /// Get a color for a tag, chosen in a pseudo-random way (from the hash code of the tag) among a palette of possible colors
        /// </summary>
        /// <param name="tag">The tag for which to get a color</param>
        /// <returns>The RGBA color to associate with the tag</returns>
        public static Color GetColorForTag(string tag)
        {
            try
            {
                ColorUtility.TryParseHtmlString($"#{Convert.ToString(tag.GetHashCode(), 16)}", out Color color);
                color.r = Mathf.Lerp(0, 0.9f, color.r);
                color.g = Mathf.Lerp(0, 0.9f, color.g);
                color.b = Mathf.Lerp(0, 0.9f, color.b);
                color.a = 1;

                return color;
            }
            catch (UnityException)
            {
                System.Random rand = new System.Random(tag.GetHashCode());
                return new Color((float)rand.NextDouble() * 0.9f, (float)rand.NextDouble() * 0.9f, (float)rand.NextDouble() * 0.9f);
            }
        }
    }
}
