using GameEngine.Core.Serialization.Text;
using GameEngine.Core.Utilities.Enums;
using System;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// A configuration that is used to customize the content service
    /// </summary>
    [Serializable]
    public class ContentConfiguration
    {
        /// <summary>
        /// Id for referencing the content configuration
        /// </summary>
        public const string CONFIG_ID = "Content";

        /// <summary>
        /// Authorize content data management if the project requires it
        /// </summary>
        public bool EnableContentData;

        /// <summary>
        /// The path to the folder containing the content
        /// </summary>
        public string DataContentPath;

        /// <summary>
        /// The serialization format of the content
        /// </summary>
        public SerializerFormat FileContentFormat;

        /// <summary>
        /// The encoding format of the characters
        /// </summary>
        public EncodingType FileEncodingType;
    }
}
