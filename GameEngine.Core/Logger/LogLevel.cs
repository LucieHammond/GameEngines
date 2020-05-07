namespace GameEngine.Core.Logger
{
    /// <summary>
    /// A level associated to a log message that gives a rough guide to the importance and urgency of the message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Relatively detailed tracing message to be used for debugging
        /// </summary>
        Debug,

        /// <summary>
        /// Informational message that highlight the progress of the program
        /// </summary>
        Info,

        /// <summary>
        /// Message alerting of a potentially harmful situation that may create problems
        /// </summary>
        Warning,

        /// <summary>
        /// Messages notifying events of considerable importance that will prevent normal program execution
        /// </summary>
        Error,

        /// <summary>
        /// Message used in case of severe error event causing the program to terminate
        /// </summary>
        Fatal
    }
}
