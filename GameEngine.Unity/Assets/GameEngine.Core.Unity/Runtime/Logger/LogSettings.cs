using GameEngine.Core.Logger;
using GameEngine.Core.Logger.Base;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace GameEngine.Core.Unity.Logger
{
    /// <summary>
    /// Settings for the runtime logs in Unity, allowing customization of display and filtering
    /// </summary>
    public class LogSettings : ScriptableObject
    {
        /// <summary>
        /// The name of the asset that should store the Log Settings
        /// </summary>
        public const string ASSET_NAME = "LogSettings";

        /// <summary>
        /// The minimal level of logs to display in Unity console. All logs with an inferior level of importance will be ignored
        /// </summary>
        public LogLevel MinLogLevel;

        /// <summary>
        /// Whether or not to filter the logs on their tags in Unity console
        /// </summary>
        public bool ActivateFiltering;

        /// <summary>
        /// The tags on which to filter the logs in Unity console. Only the logs corresponding to the tags in the list are displayed
        /// </summary>
        public HashSet<string> TagsFilter;

        /// <summary>
        /// The colors to use when displaying the specific associated tags in Unity console
        /// </summary>
        public Dictionary<string, Color> TagsColors;

        /// <summary>
        /// Other logger types to use besides the Unity console (with their own parameters and filtering policies)
        /// </summary>
        public Dictionary<BaseLoggerType, object[]> AdditionalLoggers;

        /// <summary>
        /// Initialize a new instance of LogSettings
        /// </summary>
        public LogSettings()
        {
            MinLogLevel = LogLevel.Debug;
            ActivateFiltering = false;
            TagsFilter = new HashSet<string>();
            TagsColors = new Dictionary<string, Color>();
            AdditionalLoggers = new Dictionary<BaseLoggerType, object[]>();
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
                color.r = Mathf.Lerp(0.1f, 1, color.r);
                color.g = Mathf.Lerp(0.1f, 1, color.g);
                color.b = Mathf.Lerp(0.1f, 1, color.b);
                color.a = 1;

                return color;
            }
            catch (UnityException)
            {
                Random rand = new Random(tag.GetHashCode());
                return new Color(0.1f + 0.9f * (float)rand.NextDouble(), 0.1f + 0.9f * (float)rand.NextDouble(), 0.1f + 0.9f * (float)rand.NextDouble());
            }
        }
    }
}
