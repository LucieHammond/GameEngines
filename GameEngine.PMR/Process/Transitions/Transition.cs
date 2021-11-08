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
        /// An object giving time information about the process pace (delta time, frame count, time since startup ...)
        /// </summary>
        protected ITime m_Time;

        /// <summary>
        /// A floating number between 0 and 1 representing the progress of the loading process occuring during transition
        /// </summary>
        protected float m_LoadingProgress;

        /// <summary>
        /// A small text indicating a relevant action of the loading process occuring at the same time
        /// </summary>
        protected string m_LoadingAction = "";

        /// <summary>
        /// If the loading progress should be set by default to the proportion of initialized rules in the corresponding module
        /// </summary>
        protected bool m_UseDefaultReport = true;

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

        internal void SetDefaultProgress(float progress)
        {
            if (m_UseDefaultReport)
                m_LoadingProgress = progress;
        }

        internal void BaseInitialize(ITime time)
        {
            m_Time = time;
            State = TransitionState.Inactive;

            Initialize();
        }

        internal void BaseEnter()
        {
            m_LoadingProgress = 0;
            m_LoadingAction = "";

            State = TransitionState.Activating;
            Enter();
        }

        internal void BaseUpdate()
        {
            Update();
        }

        internal void BaseExit()
        {
            m_LoadingProgress = 1;

            State = TransitionState.Deactivating;
            Exit();
        }

        internal void BaseCleanup()
        {
            State = TransitionState.Inactive;
            Cleanup();
        }

        /// <summary>
        /// The place where to define custom initializing operations for the transition. Will be called first
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// The place where to define custom activation operations for the transition (usually fade in)
        /// </summary>
        protected abstract void Enter();

        /// <summary>
        /// The place where to define custom update operations for the transition. Will be called every frame including during enter and exit
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// The place where to define custom deactivation operations for the transition (usually fade out)
        /// </summary>
        protected abstract void Exit();

        /// <summary>
        /// The place where to define custom cleanup operations for the transition. Will be called last
        /// </summary>
        protected abstract void Cleanup();

        /// <summary>
        /// Call this method to attest that the transition has complete its activation sequence
        /// </summary>
        protected void MarkActivated()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Activating)
                throw new InvalidOperationException($"Invalid time context for calling MarkActivated(). " +
                    $"Current state: {State}. Expected state: Activating");
#endif
            State = TransitionState.Active;
        }

        /// <summary>
        /// Call this method to attest that the transition has complete its deactivation sequence
        /// </summary>
        protected void MarkDeactivated()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Deactivating)
                throw new InvalidOperationException($"Invalid time context for calling MarkDeactivated(). " +
                    $"Current state: {State}. Expected state: Deactivating");
#endif
            State = TransitionState.Inactive;
        }
    }
}
