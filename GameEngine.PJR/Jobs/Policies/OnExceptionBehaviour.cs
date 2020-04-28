namespace GameEngine.PJR.Jobs.Policies
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
        /// The GameJob is paused, which means it stays in the same state as it was at the moment of the error
        /// Any new update of the GameJob is skipped until restart but the rest of the process continues running
        /// </summary>
        PauseJob,

        /// <summary>
        /// The GameJob is unloaded. If the job is a GameMode and a fallback mode is defined, this mode is loaded instead
        /// </summary>
        UnloadJob,

        /// <summary>
        /// The parent GameProcess is paused. The whole program is frozen in the state it was at the moment of the error
        /// </summary>
        PauseAll,

        /// <summary>
        /// The parent GameProcess is stopped, which means the CurrentGameMode and the ServiceHandler are unloaded and the application quits
        /// </summary>
        StopAll
    }
}
