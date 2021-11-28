using System;

namespace GameEngine.Core.UnityEditor.Build.AssetBundles
{
    /// <summary>
    /// A custom definition of an asset bundle to create
    /// </summary>
    [Serializable]
    public class AssetBuildPlan
    {
        /// <summary>
        /// Create an instance of AssetBuildPlan
        /// </summary>
        public AssetBuildPlan()
        {
            BundleName = "NewBundle";
            AssetNames = new string[0];
        }

        /// <summary>
        /// The name of the asset bundle
        /// </summary>
        public string BundleName;

        /// <summary>
        /// The references of the assets to be included in the bundle (as paths in the Asset directory)
        /// </summary>
        public string[] AssetNames;
    }
}
