namespace GameEngine.PSMR.Modes.Policies
{
    /// <summary>
    /// All possible kind of actions that can be taken when an error is detected
    /// </summary>
    public enum OnErrorBehaviour
    {
        /// <summary>
        /// Nothing is done, the process continues as if nothing ever happened
        /// </summary>
        Continue,

        /// <summary>
        /// The process continues but the rest of the frame is skipped. The following rules will not be updated
        /// </summary>
        SkipFrame,

        /// <summary>
        /// The GameMode is paused, which means it stays in the same state as it was at the moment of the error
        /// Any new update of the GameMode is skipped until restart but the rest of the process continues running
        /// </summary>
        PauseMode,

        /// <summary>
        /// The GameMode is unloaded and if a fallback mode is defined, this mode is loaded instead
        /// </summary>
        UnloadMode,

        /// <summary>
        /// The parent GameProcess is paused. The whole program (services and game modes) is frozen in the state it was at the moment of the error
        /// </summary>
        PauseAll,

        /// <summary>
        /// The parent GameProcess is stopped, which means the GameMode and the ServiceMode are unloaded and the application quits
        /// </summary>
        StopAll
    }
}
