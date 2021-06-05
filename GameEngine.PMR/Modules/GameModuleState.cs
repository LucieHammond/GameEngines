namespace GameEngine.PMR.Modules
{
    /// <summary>
    /// All possible states a GameModule can go through during its lifetime
    /// Those states are visited by a FSM associated with the GameModule
    /// </summary>
    public enum GameModuleState
    {
        /// <summary>
        /// The module is configured using the IGameModuleSetup passed at construction
        /// </summary>
        Setup,

        /// <summary>
        /// The module fills all dependencies needed by its rules
        /// </summary>
        DependencyInjection,

        /// <summary>
        /// The module initialize its rules in the order specified by the setup
        /// </summary>
        InitializeRules,

        /// <summary>
        /// The module update its rules frame after frame following a schedule specified by the setup
        /// </summary>
        UpdateRules,

        /// <summary>
        /// The module unload its rules in the order specified by the setup
        /// </summary>
        UnloadRules,

        /// <summary>
        /// The module waits to be closed by its parent process
        /// </summary>
        End
    }
}
