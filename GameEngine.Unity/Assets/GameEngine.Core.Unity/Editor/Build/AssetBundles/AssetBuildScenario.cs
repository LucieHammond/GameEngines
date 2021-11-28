using System;
using UnityEditor;

namespace GameEngine.Core.UnityEditor.Build.AssetBundles
{
    /// <summary>
    /// A scenario for creating and exporting asset bundles (targeting a particular platform for example)
    /// </summary>
    [Serializable]
    public class AssetBuildScenario
    {
        /// <summary>
        /// The name of the scenario (for Editor display)
        /// </summary>
        public string Name;

        /// <summary>
        /// If the scenario is currently activated for the next build
        /// </summary>
        public bool Activated;

        /// <summary>
        /// The path of the output directory where to export the asset bundles
        /// </summary>
        public string OutputPath;

        /// <summary>
        /// The asset bundle building options, used as flags
        /// </summary>
        public BuildAssetBundleOptions BuildOptions;

        /// <summary>
        /// The target platform for the build
        /// </summary>
        public BuildTarget TargetPlatform;
    }
}
