namespace GameEngine.PMR.Modules.Policies
{
    /// <summary>
    /// Set of module configurations concerning exceptions handling
    /// </summary>
    public class ExceptionPolicy
    {
        /// <summary>
        /// Kind of action to take during InjectDependencies or InitializeRules when an exception is catched
        /// </summary>
        public OnExceptionBehaviour ReactionDuringLoad;

        /// <summary>
        /// Kind of action to take during UpdateRules when an exception is catched
        /// </summary>
        public OnExceptionBehaviour ReactionDuringUpdate;

        /// <summary>
        /// Kind of action to take during UnloadRules when an exception is catched
        /// </summary>
        public OnExceptionBehaviour ReactionDuringUnload;

        /// <summary>
        /// Indicate if the unload method of a GameRule can be skipped in case an exception occurs in it
        /// </summary>
        public bool SkipUnloadIfException;

        /// <summary>
        /// The module to load instead of the current one if it needs to be unloaded due to exceptions or reported errors
        /// </summary>
        public IGameModuleSetup FallbackModule;
    }
}
