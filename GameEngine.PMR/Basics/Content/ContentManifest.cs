using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Basics.Content
{
    /// <summary>
    /// A manifest describing all the content that has been placed in the build
    /// </summary>
    [Serializable]
    public class ContentManifest
    {
        /// <summary>
        /// The version of the content
        /// </summary>
        public Version Version;

        /// <summary>
        /// The platform for which the content has been created
        /// </summary>
        public string Platform;

        /// <summary>
        /// An index matching all content ids to their corresponding filenames
        /// </summary>
        public Dictionary<string, string> FileNames;
    }
}
