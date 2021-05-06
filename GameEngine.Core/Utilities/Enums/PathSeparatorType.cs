namespace GameEngine.Core.Utilities.Enums
{
    /// <summary>
    /// Type of directory separator character to use in paths formatted as string
    /// </summary>
    public enum PathSeparatorType
    {
        /// <summary>
        /// Standard platform-specific directory separator
        /// </summary>
        StdSeparator,

        /// <summary>
        /// Alternative platform-specific directory separator
        /// </summary>
        AltSeparator,

        /// <summary>
        /// Forward Slash separator ('/')
        /// </summary>
        ForwardSlash,

        /// <summary>
        /// BackSlash separator ('\')
        /// </summary>
        BackSlash
    }
}
