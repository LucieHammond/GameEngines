using GameEngine.Core.Utilities.Enums;
using System.Text;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for encoding operations
    /// </summary>
    public static class EncodingUtils
    {
        /// <summary>
        /// Create an Encoding object corresponding to the given format
        /// </summary>
        /// <param name="encodingType">The character encoding format to use</param>
        /// <returns>An appropriate instance of System.Text.Encoding</returns>
        public static Encoding CreateEncoding(EncodingType encodingType)
        {
            switch(encodingType)
            {
                case EncodingType.UTF8:
                    return Encoding.UTF8;
                case EncodingType.UTF7:
                    return Encoding.UTF7;
                case EncodingType.UTF32:
                    return Encoding.UTF32;
                case EncodingType.UTF16:
                    return Encoding.Unicode;
                case EncodingType.ASCII:
                    return Encoding.ASCII;
                default:
                    return Encoding.Default;
            }
        }
    }
}
