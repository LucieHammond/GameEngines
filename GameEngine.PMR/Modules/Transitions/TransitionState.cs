namespace GameEngine.PMR.Modules.Transitions
{
    /// <summary>
    /// All possible states a TransitionActivity can take during its lifecycle
    /// </summary>
    public enum TransitionState
    {
        /// <summary>
        /// The transition is inactive
        /// </summary>
        Inactive,

        /// <summary>
        /// The transition is in starting phase, usually used for fade in
        /// </summary>
        Starting,

        /// <summary>
        /// The transition is currently running and fully displayed
        /// </summary>
        Active,

        /// <summary>
        /// The transition is in stopping phase, usually used for fade out
        /// </summary>
        Stopping,
    }
}
