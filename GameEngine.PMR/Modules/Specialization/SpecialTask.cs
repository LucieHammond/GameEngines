using GameEngine.PMR.Rules;
using System;

namespace GameEngine.PMR.Modules.Specialization
{
    /// <summary>
    /// Abstract template representing a specialized configuration task to apply to a module. Each task figuring in the module setup must implement this class
    /// </summary>
    public abstract class SpecialTask
    {
        /// <summary>
        /// The state of the task
        /// </summary>
        public SpecialTaskState State { get; private set; }

        /// <summary>
        /// Default constructor of a SpecialTask
        /// </summary>
        public SpecialTask()
        {
            State = SpecialTaskState.Created;
        }

        /// <summary>
        /// Get an estimate of the progress of the running task
        /// </summary>
        /// <returns>A floating number between 0 and 1 representing the progress of the task</returns>
        public abstract float GetProgress();

        internal void BaseInitialize(RulesDictionary rules)
        {
            State = SpecialTaskState.InitRunning;
            Initialize(rules);
        }

        internal void BaseUnload(RulesDictionary rules)
        {
            State = SpecialTaskState.UnloadRunning;
            Unload(rules);
        }

        internal void BaseUpdate(int maxDuration)
        {
            Update(maxDuration);
        }

        /// <summary>
        /// The place where to launch the initialization phase of the task, applied to the module rules before their individual initialization
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected abstract void Initialize(RulesDictionary rules);

        /// <summary>
        /// The place where to launch the unload phase of the task, applied to the module rules after their individual unload
        /// </summary>
        /// <param name="rules">A dictionary containing the rules of the module</param>
        protected abstract void Unload(RulesDictionary rules);

        /// <summary>
        /// The place where to update the task if necessary, without exceeding the given maximum duration
        /// </summary>
        /// <param name="maxDuration">The maximum time (in ms) the update should take</param>
        protected abstract void Update(int maxDuration);

        /// <summary>
        /// Call this method to attest that the task has finished its initialization phase. This mecanism allows asynchronous initialization
        /// </summary>
        protected void FinishInitialization()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != SpecialTaskState.InitRunning)
                throw new InvalidOperationException($"Invalid time context for calling FinishInitialization(). Current state: {State}. Expected state: InitRunning");
#endif
            State = SpecialTaskState.InitCompleted;
        }

        /// <summary>
        /// Call this method to attest that the task has finished its unload phase. This mecanism allows asynchronous unload
        /// </summary>
        protected void FinishUnload()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != SpecialTaskState.UnloadRunning)
                throw new InvalidOperationException($"Invalid time context for calling FinishUnload(). Current state: {State}. Expected state: UnloadRunning");
#endif
            State = SpecialTaskState.UnloadCompleted;
        }
    }
}
