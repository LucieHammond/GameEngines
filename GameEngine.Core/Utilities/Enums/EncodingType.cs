namespace GameEngine.Core.Utilities.Enums
{
    /// <summary>
    /// Different types of character encoding formats for texts
    /// </summary>
    public enum EncodingType
    {
        /// <summary>
        /// UTF-8 character format
        /// </summary>
        UTF8,

        /// <summary>
        /// UTF-7 character format
        /// </summary>
        UTF7,

        /// <summary>
        /// UTF-32 character format using the little endian byte order
        /// </summary>
        UTF32,

        /// <summary>
        /// UTF-16 character format using the little endian byte order
        /// </summary>
        UTF16,

        /// <summary>
        /// ASCII (7-bit) character format
        /// </summary>
        ASCII
    }
}
