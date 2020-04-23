namespace GameEngine.PSMR.Modes
{
    /// <summary>
    /// All possible states a GameMode can go through during its lifetime
    /// Those states are visited by a FSM associated with the GameMode
    /// </summary>
    public enum GameModeState
    {
        /// <summary>
        /// The GameMode is configured using the GameModeSetup passed at construction
        /// </summary>
        Setup,

        /// <summary>
        /// The GameMode fills all dependencies needed by its rules
        /// </summary>
        DependencyInjection,

        /// <summary>
        /// The GameMode initialize its rules in the order specified by the setup
        /// </summary>
        InitializeRules,

        /// <summary>
        /// The GameMode update its rules frame after frame following a schedule specified by the setup
        /// </summary>
        UpdateRules,

        /// <summary>
        /// The GameMode unload its rules in the order specified by the setup
        /// </summary>
        UnloadRules,

        /// <summary>
        /// The GameMode waits to be closed by its parent process
        /// </summary>
        End
    }
}
