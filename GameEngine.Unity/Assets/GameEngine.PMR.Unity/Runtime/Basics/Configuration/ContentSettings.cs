using GameEngine.Core.Unity.System;
using GameEngine.PMR.Unity.Basics.Content;
using System.IO;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Configuration
{
    /// <summary>
    /// Settings of the game content in Unity, describing where it is stored and in what format
    /// </summary>
    public class ContentSettings : ScriptableSettings<UnityContentConfiguration>
    {
        /// <summary>
        /// The name of the asset that should store the Content Settings
        /// </summary>
        public const string ASSET_NAME = "ContentSettings";

        /// <summary>
        /// A unique identifier used to store and retrieve this configuration
        /// </summary>
        public const string CONFIG_ID = UnityContentConfiguration.CONFIG_ID;

        /// <summary>
        /// Get the content settings that have been defined in the project through a dedicated asset (or default if not defined)
        /// </summary>
        /// <returns>The requested instance of ContentSettings</returns>
        public static ContentSettings GetSettings()
        {
            ContentSettings settings = Resources.Load<ContentSettings>(ASSET_NAME);
            if (settings == null)
                settings = CreateInstance<ContentSettings>();

            return settings;
        }

        /// <summary>
        /// Check if the content configuration is valid
        /// </summary>
        /// <returns>True if the configuration is valid, else False</returns>
        public override bool Validate()
        {
            if (Configuration.EnableContentData)
            {
                if (string.IsNullOrEmpty(Configuration.DataContentPath) || !Directory.Exists(Configuration.DataContentPath))
                    return false;
            }

            if (Configuration.EnableContentDescriptors)
            {
                if (string.IsNullOrEmpty(Configuration.DescriptorContentPath) || !Directory.Exists(Configuration.DescriptorContentPath))
                    return false;

                if (string.IsNullOrEmpty(Configuration.DescriptorContentPath))
                    return false;
            }

            if (Configuration.EnableContentAssets)
            {
                if (!string.IsNullOrEmpty(Configuration.AssetContentPath) && !Directory.Exists(Configuration.AssetContentPath))
                    return false;
            }

            return true;
        }
    }
}
