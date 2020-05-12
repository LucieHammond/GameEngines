using System;

namespace GameEngine.Core.FSM
{
    /// <summary>
    /// A FSM state associated with a specific value of the T Enum describing all FSM states.
    /// </summary>
    /// <typeparam name="T">An enum describing all possible states of the FSM.</typeparam>
    public abstract class FSMState<T> where T : Enum
    {
        /// <summary>
        /// The state id, which is the value of the T Enum corresponding to the state.
        /// </summary>
        public abstract T Id { get; }

        /// <summary>
        /// A name identifying the state and the FSM it is part of.
        /// </summary>
        public string Name
        {
            get => m_FSM != null ? $"{m_FSM.Name}_{Id}" : $"detached_{Id}";
        }

        /// <summary>
        /// A reference to the FSM in which the state is included.
        /// </summary>
        protected FSM<T> m_FSM;

        /// <summary>
        /// Put here all initialization operations. Called when the FSM starts.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Put here all unloading operations. Called when the FSM stops.
        /// </summary>
        public virtual void Unload()
        {

        }

        /// <summary>
        /// Define beginning behaviour. Called each time the FSM enters the state.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Define update behaviour. Called at each frame during the time the FSM is inside the state.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Define end behaviour. Called each time the FSM exits the state.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// A shortcut that calls the method of the FSM with same signature.
        /// </summary>
        /// <seealso cref="GameEngine.FSM.FSM.SetState"/>
        protected void SetState(T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            m_FSM.SetState(stateId, immediate, ignoreIfCurrentState, priority);
        }

        internal void AttachToFSM(FSM<T> stateMachine)
        {
            m_FSM = stateMachine;
        }
    }
}
