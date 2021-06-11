namespace GameEngine.PMR.Modules
{
    /// <summary>
    /// All possible states a GameModule can go through during its lifetime.
    /// Those states are visited by a FSM associated with the GameModule
    /// </summary>
    public enum GameModuleState
    {
        /// <summary>
        /// The module is created and waits to be loaded by its orchestrator
        /// </summary>
        Start,

        /// <summary>
        /// The module is configured using the IGameModuleSetup passed at construction
        /// </summary>
        Setup,

        /// <summary>
        /// The mudule fills all dependencies needed by its rules
        /// </summary>
        InjectDependencies,

        /// <summary>
        /// The module initialize its rules in the order specified by the setup
        /// </summary>
        InitializeRules,

        /// <summary>
        /// The module update its rules frame after frame following a schedule specified by the setup
        /// </summary>
        UpdateRules,

        /// <summary>
        /// The module unload its rules in the reverse order specified by the setup
        /// </summary>
        UnloadRules,

        /// <summary>
        /// The module waits to be stopped and destroyed by its orchestrator
        /// </summary>
        End,
    }
}
