using GameEngine.Core.System;
using GameEngine.Core.Utilities;
using System;

namespace GameEngine.PMR.Process.Transitions
{
    /// <summary>
    /// Abstract template representing a transition. Each custom transition in a project must inherit from this class
    /// </summary>
    public abstract class Transition
    {
        /// <summary>
        /// The state of the transition
        /// </summary>
        public TransitionState State { get; private set; }

        /// <summary>
        /// Indicates if the transition has been loaded and is ready to be launched
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Indicates if the transition display is complete and can be ended
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Whether the associated module should be updated during the transition entry
        /// </summary>
        public abstract bool UpdateDuringEntry { get; }

        /// <summary>
        /// Whether the associated module should be updated during the transition exit
        /// </summary>
        public abstract bool UpdateDuringExit { get; }

        /// <summary>
        /// An object giving time information about the process pace (delta time, frame count, time since startup ...)
        /// </summary>
        protected ITime m_Time;

        /// <summary>
        /// The configuration of the module being loaded, which contains runtime information
        /// </summary>
        protected Configuration m_ModuleConfiguration;

        /// <summary>
        /// A floating number between 0 and 1 representing the progress of the loading process occuring during transition
        /// </summary>
        protected float m_LoadingProgress;

        /// <summary>
        /// A small text indicating a relevant action of the loading process occuring at the same time
        /// </summary>
        protected string m_LoadingAction = "";

        /// <summary>
        /// Configure the transition with settings relative to the process and the module being loaded
        /// </summary>
        /// <param name="time">The timing object of the process</param>
        /// <param name="configuration">The configuration of the module being loaded</param>
        public void Configure(ITime time, Configuration configuration)
        {
            m_Time = time;
            m_ModuleConfiguration = configuration;
        }

        /// <summary>
        /// Report the progress of the loading process for the transition to display it
        /// </summary>
        /// <param name="progress">A floating number between 0 and 1 representing the progress</param>
        public void ReportLoadingProgress(float progress)
        {
            m_LoadingProgress = MathUtils.Clamp(progress, 0f, 1f);
        }

        /// <summary>
        /// Report a relevant action of the loading process for the transition to display it
        /// </summary>
        /// <param name="currentAction">A string representing the action</param>
        public void ReportLoadingAction(string currentAction)
        {
            m_LoadingAction = currentAction;
        }

        internal void BasePrepare()
        {
            IsReady = false;

            State = TransitionState.Inactive;
            Prepare();
        }

        internal void BaseEnter()
        {
            m_LoadingProgress = 0;
            m_LoadingAction = "";
            IsComplete = false;

            State = TransitionState.Entering;
            Enter();
        }

        internal void BaseUpdate()
        {
            Update();
        }

        internal void BaseExit()
        {
            m_LoadingProgress = 1;

            State = TransitionState.Exiting;
            Exit();
        }

        internal void BaseCleanup()
        {
            IsReady = false;

            State = TransitionState.Inactive;
            Cleanup();
        }

        /// <summary>
        /// The place where to define custom load operations for the transition. Will be called first
        /// </summary>
        protected abstract void Prepare();

        /// <summary>
        /// The place where to define custom entry operations for the transition (usually fade in)
        /// </summary>
        protected abstract void Enter();

        /// <summary>
        /// The place where to define custom update operations for the transition. Will be called every frame including during enter and exit
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// The place where to define custom exit operations for the transition (usually fade out)
        /// </summary>
        protected abstract void Exit();

        /// <summary>
        /// The place where to define custom unload operations for the transition. Will be called last
        /// </summary>
        protected abstract void Cleanup();

        /// <summary>
        /// Call this method to attest that the transition has correctly been setup
        /// </summary>
        protected void MarkReady()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Inactive)
                throw new InvalidOperationException($"Invalid time context for calling MarkReady(). " +
                    $"Current state: {State}. Expected state: Inactive");
#endif
            IsReady = true;
        }

        /// <summary>
        /// Call this method to attest that the transition has completed its entry sequence
        /// </summary>
        protected void MarkEntered()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Entering)
                throw new InvalidOperationException($"Invalid time context for calling MarkEntered(). " +
                    $"Current state: {State}. Expected state: Entering");
#endif
            State = TransitionState.Running;
        }

        /// <summary>
        /// Call this method to attest that the transition has completed its exit sequence
        /// </summary>
        protected void MarkExited()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Exiting)
                throw new InvalidOperationException($"Invalid time context for calling MarkExited(). " +
                    $"Current state: {State}. Expected state: Exiting");
#endif
            State = TransitionState.Inactive;
        }

        /// <summary>
        /// Call this method to attest that the transition display requirements have been completed
        /// </summary>
        protected void MarkCompleted()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Running && State != TransitionState.Entering)
                throw new InvalidOperationException($"Invalid time context for calling MarkCompleted(). " +
                    $"Current state: {State}. Expected state: Running, Entering");
#endif
            IsComplete = true;
        }
    }
}
