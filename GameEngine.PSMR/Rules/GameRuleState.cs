namespace GameEngine.PSMR.Rules
{
    /// <summary>
    /// All possible states a GameRule can take during its lifecycle
    /// </summary>
    public enum GameRuleState
    {
        /// <summary>
        /// The rule is created but not yet initialized. Only the constructor has been called
        /// </summary>
        Unused,

        /// <summary>
        /// The rule is being initialized. The initialization process has been started but is not yet finished
        /// </summary>
        Initializing,

        /// <summary>
        /// The rule has finished its initialization process. It is fully operational for updates
        /// </summary>
        Initialized,

        /// <summary>
        /// The rule is being unloaded. The unloading process has been started but is not yet finished
        /// </summary>
        Unloading,

        /// <summary>
        /// The rule has finished its unloading process. It is ready for destruction
        /// </summary>
        Unloaded
    }
}
