namespace GameEngine.PMR.Process.Orchestration
{
    /// <summary>
    /// All possible states an Orchestrator can go through during its lifetime.
    /// Those states are visited by a FSM associated with the Orchestrator
    /// </summary>
    public enum OrchestratorState
    {
        /// <summary>
        /// The orchestrator waits to be linked to a module or removed
        /// </summary>
        Wait,

        /// <summary>
        /// The orchestrator executes the entry phase of the associated module transition
        /// </summary>
        EnterTransition,

        /// <summary>
        /// The orchestrator prepares the module while displaying its transition
        /// </summary>
        RunTransition,

        /// <summary>
        /// The orchestrator executes the exit phase of the associated module transition
        /// </summary>
        ExitTransition,

        /// <summary>
        /// The orchestrator manages the normal execution of the module (and its submodules)
        /// </summary>
        Operational,

        /// <summary>
        /// The orchestrator continues to execute the module while unloading its submodules
        /// </summary>
        Reset,
    }
}
