namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// All possible states a Transition can take during its lifecycle
    /// </summary>
    public enum TransitionState
    {
        /// <summary>
        /// The transition is created but inactive
        /// </summary>
        Inactive,

        /// <summary>
        /// The transition is in entry phase, usually used for fade in
        /// </summary>
        Entering,

        /// <summary>
        /// The transition is currently running and fully displayed
        /// </summary>
        Running,

        /// <summary>
        /// The transition is in exit phase, usually used for fade out
        /// </summary>
        Exiting,
    }
}
