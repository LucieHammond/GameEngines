using GameEngine.PMR.Basics.Content;
using System;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A configuration that is used to customize the different content services
    /// </summary>
    [Serializable]
    public class UnityContentConfiguration : ContentConfiguration
    {
        /// <summary>
        /// Authorize content descriptors management if the project requires it
        /// </summary>
        public bool EnableContentDescriptors;

        /// <summary>
        /// The path of the folder containing the descriptor asset bundle (usually same as AssetContentPath)
        /// </summary>
        public string DescriptorContentPath;

        /// <summary>
        /// The name of the asset bundle referencing all descriptors
        /// </summary>
        public string DescriptorBundleName;

        /// <summary>
        /// Authorize content assets management if the project requires it
        /// </summary>
        public bool EnableContentAssets;

        /// <summary>
        /// The path of the folder containing the asset bundles
        /// </summary>
        public string AssetContentPath;
    }
}
