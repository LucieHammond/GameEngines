using System;
using System.Collections.Generic;

namespace GameEngine.Core.UnityEditor.Build.AssetBundles
{
    /// <summary>
    /// The settings for building asset bundles from the Editor
    /// </summary>
    [Serializable]
    public class AssetBuildSettings
    {
        /// <summary>
        /// The name of the asset that should store the AssetBuild Settings
        /// </summary>
        public const string FILE_NAME = "AssetBuildSettings";

        /// <summary>
        /// Create an instance of AssetBuildSettings
        /// </summary>
        public AssetBuildSettings()
        {
            CustomBuildMap = new AssetBuildPlan[0];
            BuildScenarios = new List<AssetBuildScenario>();
        }

        /// <summary>
        /// If true, build all the asset bundles referenced in the Editor
        /// If false, build asset bundles according to a customly defined building map
        /// </summary>
        public bool BuildAllBundles;

        /// <summary>
        /// The customized asset bundle building map
        /// </summary>
        public AssetBuildPlan[] CustomBuildMap;

        /// <summary>
        /// The list of building scenarios
        /// </summary>
        public List<AssetBuildScenario> BuildScenarios;
    }
}
