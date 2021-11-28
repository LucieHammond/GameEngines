namespace GameEngine.PMR.Modules.Specialization
{
    /// <summary>
    /// All possible states a SpecialTask can take during its lifecycle
    /// </summary>
    public enum SpecialTaskState
    {
        /// <summary>
        /// The task has been created but is not yet running
        /// </summary>
        Created,

        /// <summary>
        /// The task is performing its initialization phase
        /// </summary>
        InitRunning,

        /// <summary>
        /// The task has successfully finished its initialization phase
        /// </summary>
        InitCompleted,

        /// <summary>
        /// The task is performing its unload phase
        /// </summary>
        UnloadRunning,

        /// <summary>
        /// The task has successfully finished its unload phase
        /// </summary>
        UnloadCompleted
    }
}
