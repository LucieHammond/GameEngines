using GameEngine.Core.System;
using System;

namespace GameEngine.PMR.Modules.Transitions
{
    /// <summary>
    /// Abstract template representing a transition. Each custom transition in a project must inherit from this class
    /// </summary>
    public abstract class TransitionActivity
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
            m_LoadingProgress = Math.Max(0, Math.Min(1, progress));
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

        internal void BaseStart()
        {
            m_LoadingProgress = 0;
            m_LoadingAction = "";

            State = TransitionState.Starting;
            Start();
        }

        internal void BaseUpdate()
        {
            Update();
        }

        internal void BaseStop()
        {
            m_LoadingProgress = 1;

            State = TransitionState.Stopping;
            Stop();
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
        /// The place where to define custom starting operations for the transition (usually fade in)
        /// </summary>
        protected abstract void Start();

        /// <summary>
        /// The place where to define custom update operations for the transition. Will be called every frame including during start and stop
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// The place where to define custom stopping operations for the transition (usually fade out)
        /// </summary>
        protected abstract void Stop();

        /// <summary>
        /// The place where to define custom cleanup operations for the transition. Will be called last
        /// </summary>
        protected abstract void Cleanup();

        /// <summary>
        /// Call this method to attest that the transition has complete its starting sequence
        /// </summary>
        protected void MarkStartCompleted()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Starting)
                throw new InvalidOperationException($"Invalid time context for calling MarkStartCompleted(). " +
                    $"Current state: {State}. Expected state: Starting");
#endif
            State = TransitionState.Active;
        }

        /// <summary>
        /// Call this method to attest that the transition has complete its stopping sequence
        /// </summary>
        protected void MarkStopCompleted()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != TransitionState.Stopping)
                throw new InvalidOperationException($"Invalid time context for calling MarkStartCompleted(). " +
                    $"Current state: {State}. Expected state: Stopping");
#endif
            State = TransitionState.Inactive;
        }
    }
}
