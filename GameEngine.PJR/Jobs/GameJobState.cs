namespace GameEngine.PJR.Jobs
{
    /// <summary>
    /// All possible states a GameJob can go through during its lifetime
    /// Those states are visited by a FSM associated with the GameJob
    /// </summary>
    public enum GameJobState
    {
        /// <summary>
        /// The GameJob is configured using the GameJobSetup passed at construction
        /// </summary>
        Setup,

        /// <summary>
        /// The GameJob fills all dependencies needed by its rules
        /// </summary>
        DependencyInjection,

        /// <summary>
        /// The GameJob initialize its rules in the order specified by the setup
        /// </summary>
        InitializeRules,

        /// <summary>
        /// The GameJob update its rules frame after frame following a schedule specified by the setup
        /// </summary>
        UpdateRules,

        /// <summary>
        /// The GameJob unload its rules in the order specified by the setup
        /// </summary>
        UnloadRules,

        /// <summary>
        /// The GameJob waits to be closed by its parent process
        /// </summary>
        End
    }
}
