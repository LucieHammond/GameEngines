namespace GameEngine.PSMR.Modes.Policies
{
    /// <summary>
    /// Set of GameMode configurations concerning exceptions and errors handling
    /// </summary>
    public class ErrorPolicy
    {
        /// <summary>
        /// If true, nothing special is done when an exception arise from a GameRule
        /// The exception is just catched and logged and the process continues as if nothing ever happened
        /// </summary>
        public bool IgnoreExceptions;

        /// <summary>
        /// Kind of action to take when an error (or unignored exception) is detected
        /// </summary>
        public OnErrorBehaviour ReactionOnError;

        /// <summary>
        /// Indicate if the unload method of a GameRule can be skipped in case an error occurs in it
        /// </summary>
        public bool SkipUnloadIfError;

        /// <summary>
        /// The GameMode to load instead of the current one if it needs to be unloaded due to errors
        /// </summary>
        public IGameModeSetup FallbackMode;
    }
}
