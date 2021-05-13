namespace GameEngine.Core.Logger.Base
{
    /// <summary>
    /// Basic logger types defined in this framework
    /// </summary>
    public enum BaseLoggerType
    {
        /// <summary>
        /// ConsoleLogger, that logs in the console
        /// </summary>
        ConsoleLogger,

        /// <summary>
        /// DebugLogger, that logs to the debug system listeners
        /// </summary>
        DebugLogger,

        /// <summary>
        /// FileLogger, that logs in a file
        /// </summary>
        FileLogger,

        /// <summary>
        /// NetLogger, that sends log messages to a net API
        /// </summary>
        NetLogger
    }
}
