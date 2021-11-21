using GameEngine.Core.Serialization.Text;
using GameEngine.Core.Utilities.Enums;
using System;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// A configuration that is used to customize the ContentService
    /// </summary>
    [Serializable]
    public class ContentConfiguration
    {
        /// <summary>
        /// The path to the folder containing the content
        /// </summary>
        public string FileContentPath;

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
