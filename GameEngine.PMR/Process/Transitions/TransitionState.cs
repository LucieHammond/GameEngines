namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// All possible states a Transition can take during its lifecycle
    /// </summary>
    public enum TransitionState
    {
        /// <summary>
        /// The transition is inactive
        /// </summary>
        Inactive,

        /// <summary>
        /// The transition is in activating phase, usually used for fade in
        /// </summary>
        Activating,

        /// <summary>
        /// The transition is currently running and fully displayed
        /// </summary>
        Active,

        /// <summary>
        /// The transition is in deactivating phase, usually used for fade out
        /// </summary>
        Deactivating,
    }
}
