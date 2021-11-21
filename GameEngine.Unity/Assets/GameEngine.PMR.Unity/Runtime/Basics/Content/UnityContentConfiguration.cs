using GameEngine.PMR.Basics.Content;
using System;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A configuration that is used to customize the UnityContentService
    /// </summary>
    [Serializable]
    public class UnityContentConfiguration : ContentConfiguration
    {
        /// <summary>
        /// The path of the folder containing the asset bundes
        /// </summary>
        public string AssetBundlesPath;

        /// <summary>
        /// The name of the asset bundle referencing all descriptors
        /// </summary>
        public string DescriptorsBundle;
    }
}
