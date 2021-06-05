namespace GameEngine.PMR.Process.Orchestration
{
    /// <summary>
    /// All possible states a ModuleOrchestrator can go through during its lifetime.
    /// Those states are visited by a FSM associated with the ModuleOrchestrator
    /// </summary>
    public enum ModuleOrchestratorState
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
        /// The orchcestrator prepares the module while displaying its transition
        /// </summary>
        RunTransition,

        /// <summary>
        /// The orchestrator executes the exit phase of the associated module transition
        /// </summary>
        ExitTransition,

        /// <summary>
        /// The orchestrator manages the normal execution of the module (and its submodules)
        /// </summary>
        OperateModule,

        /// <summary>
        /// The orchestrator continues to execute the module while unloading its submodules
        /// </summary>
        ResetSubmodules,
    }
}
