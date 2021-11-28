using System.IO;
using UnityEditor;

namespace GameEngine.Core.UnityEditor.Build.AssetBundles
{
    /// <summary>
    /// A static class regrouping methods for building asset bundles in a Unity project
    /// </summary>
    public static class AssetBundleBuilder
    {
        /// <summary>
        /// Build all asset bundles that have changed according to the given build settings
        /// </summary>
        /// <param name="buildSettings">The build settings to use</param>
        public static void BuildAssetBundles(AssetBuildSettings buildSettings)
        {
            foreach (AssetBuildScenario buildScenario in buildSettings.BuildScenarios)
            {
                if (!buildScenario.Activated)
                    continue;

                if (buildSettings.BuildAllBundles)
                {
                    BuildPipeline.BuildAssetBundles(buildScenario.OutputPath, buildScenario.BuildOptions, buildScenario.TargetPlatform);
                }
                else
                {
                    AssetBundleBuild[] customBuildMap = new AssetBundleBuild[buildSettings.CustomBuildMap.Length];
                    for (int i = 0; i < customBuildMap.Length; i++)
                    {
                        customBuildMap[i] = new AssetBundleBuild()
                        {
                            assetBundleName = buildSettings.CustomBuildMap[i].BundleName,
                            assetNames = buildSettings.CustomBuildMap[i].AssetNames,
                        };
                    }
                    BuildPipeline.BuildAssetBundles(buildScenario.OutputPath, customBuildMap, buildScenario.BuildOptions, buildScenario.TargetPlatform);
                }
            }
        }

        /// <summary>
        /// Delete all asset bundles and build them again regardless of whether or not they have changed
        /// </summary>
        /// <param name="buildSettings">The build settings to use</param>
        public static void RebuildAssetBundles(AssetBuildSettings buildSettings)
        {
            CleanAssetBundles(buildSettings);
            BuildAssetBundles(buildSettings);
        }

        /// <summary>
        /// Delete all previously created asset bundles in the directories specified by the build settings
        /// </summary>
        /// <param name="buildSettings">The build settings to use</param>
        public static void CleanAssetBundles(AssetBuildSettings buildSettings)
        {
            foreach (AssetBuildScenario buildScenario in buildSettings.BuildScenarios)
            {
                if (!buildScenario.Activated)
                    continue;

                DirectoryInfo directory = new DirectoryInfo(buildScenario.OutputPath);

                foreach (FileInfo file in directory.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in directory.EnumerateDirectories())
                {
                    dir.Delete(true);
                }
            }
        }
    }
}
