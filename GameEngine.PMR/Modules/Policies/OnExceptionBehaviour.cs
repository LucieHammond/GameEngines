namespace GameEngine.PMR.Modules.Policies
{
    /// <summary>
    /// All possible kind of actions that can be taken when an error is detected
    /// </summary>
    public enum OnExceptionBehaviour
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
        /// The module is paused, which means it stays in the same state as it was at the moment of the error.
        /// Any new update of the module is skipped until restart but the rest of the process continues running
        /// </summary>
        PauseModule,

        /// <summary>
        /// The module is unloaded, stopped and dereferenced by its parents
        /// </summary>
        UnloadModule,

        /// <summary>
        /// The module is reloaded, which means all its rules are unloaded and initialized again
        /// </summary>
        ReloadModule,

        /// <summary>
        /// The module is unloaded and the predefined fallback module is loaded instead
        /// </summary>
        SwitchToFallback,

        /// <summary>
        /// The main process is paused. The whole program is frozen in the state it was at the moment of the error
        /// </summary>
        PauseAll,

        /// <summary>
        /// The main process is stopped, which means the CurrentGameMode and the Services are unloaded and the application quits
        /// </summary>
        StopAll
    }
}
