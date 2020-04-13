using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.FSM
{
    /// <summary>
    /// A Finite State Machine associated with an enum T of possible states (see https://en.wikipedia.org/wiki/Finite-state_machine)
    /// </summary>
    /// <typeparam name="T">An enum describing all possible states of this state machine.</typeparam>
    public class FSM<T> where T : Enum
    {
        /// <summary>
        /// The name of the FSM.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The current state's id, which is the value of the T Enum corresponding to this state.
        /// </summary>
        public T CurrentStateId { get; private set; }

        /// <summary>
        /// The current FSM state.
        /// </summary>
        public FSMState<T> CurrentState
        {
            get => m_States[CurrentStateId];
        }

        /// <summary>
        /// The duration of the current state, which is the total time elapsed since the FSM entered the state, in seconds.
        /// </summary>
        public double CurrentStateDuration
        {
            get => m_CurrentStateTimeWatch.Elapsed.TotalSeconds;
        }

#if CHECK_OPERATIONS_CONTEXT
        private bool m_Running;
#endif
        private Dictionary<T, FSMState<T>> m_States;

        private bool m_StateChangeRequested;
        private byte m_StateChangePriority;
        private T m_NextStateId;

        private Stopwatch m_CurrentStateTimeWatch;

        /// <summary>
        /// Constructor of the FSM.
        /// </summary>
        /// <param name="name">The name of the FSM.</param>
        /// <param name="states">An IEnumerable containing all the states of the FSM.</param>
        /// <param name="initialStateId">The initial state of the FSM.</param>
        public FSM(string name, IEnumerable<FSMState<T>> states, T initialStateId)
        {
            Name = name;
            m_States = new Dictionary<T, FSMState<T>>();
            m_StateChangeRequested = false;
#if CHECK_OPERATIONS_CONTEXT
            m_Running = false;
#endif

            foreach (FSMState<T> state in states)
            {
                if (m_States.ContainsKey(state.StateId))
                    throw new ArgumentException($"A state machine cannot have multiple states with same id {state.StateId}", "states");

                m_States.Add(state.StateId, state);
                state.AttachToFSM(this);
            }

            if (!m_States.ContainsKey(initialStateId))
                throw new ArgumentException($"The initial state {initialStateId} is not a valid state", "initialStateId");

            CurrentStateId = initialStateId;
        }

        /// <summary>
        /// Start the FSM : initialize all states and enter the initial state.
        /// </summary>
        public void Start()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (m_Running)
                throw new InvalidOperationException($"The state machine is already started");
            m_Running = true;
#endif

            foreach (KeyValuePair<T, FSMState<T>> stateEntry in m_States)
            {
                stateEntry.Value.Initialize();
            }

            CurrentState.Enter();
            m_CurrentStateTimeWatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Update the FSM : update the current state and change state if requested.
        /// Should be called one time per frame.
        /// </summary>
        public void Update()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before Update");
#endif

            CurrentState.Update();

            if (m_StateChangeRequested)
            {
                SwitchToNextState();
            }
        }

        /// <summary>
        /// Stop the FSM : exit the current state and unload all states
        /// </summary>
        public void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before Stop");
            m_Running = false;
#endif

            m_CurrentStateTimeWatch.Stop();
            CurrentState.Exit();

            foreach (KeyValuePair<T, FSMState<T>> stateEntry in m_States)
            {
                stateEntry.Value.Unload();
            }
        }

        /// <summary>
        /// Reset the FSM to a new initial state. The FSM should be stopped.
        /// </summary>
        /// <param name="resetStateId"></param>
        public void Reset(T resetStateId)
        {
#if CHECK_OPERATIONS_CONTEXT
            if (m_Running)
                throw new InvalidOperationException($"The state machine cannot be reset while running");
#endif

            if (!m_States.ContainsKey(resetStateId))
                throw new ArgumentException($"The reset state {resetStateId} is not a valid state", "resetStateId");
        }

        /// <summary>
        /// Request a state change.
        /// </summary>
        /// <param name="stateId">The id of the requested state (enum value)</param>
        /// <param name="immediate">If the change should be applied immediatly or if it should wait the end of the next update. Default = false</param>
        /// <param name="ignoreIfCurrentState">If true, the method does nothing when the requested state is the same as the current state. If false, it exits and enters that same state. Default = false</param>
        /// <param name="priority">The priority of the state change request, between 0 (lowest priority) and 255 (highest priority). Used to decide between simultaneous requests. Default = 10</param>
        public void SetState(T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before changing state");
#endif

            if (!m_States.ContainsKey(stateId))
                throw new ArgumentException($"The state {stateId} is not part of the state machine", "stateId");

            if (ignoreIfCurrentState && stateId.Equals(CurrentStateId))
                return;

            if (m_StateChangeRequested)
            {
                if (priority < m_StateChangePriority)
                    return;
                else if (priority == m_StateChangePriority)
                    throw new InvalidOperationException($"The state machine cannot switch to {stateId} because it is already changing state to {m_NextStateId} (same priority operation)");
            }

            m_StateChangeRequested = true;
            m_NextStateId = stateId;
            m_StateChangePriority = priority;

            if (immediate)
            {
                SwitchToNextState();
            }
        }

        private void SwitchToNextState()
        {
            m_CurrentStateTimeWatch.Stop();
            CurrentState.Exit();

            CurrentStateId = m_NextStateId;
            m_StateChangeRequested = false;
            m_NextStateId = default;
            m_StateChangePriority = 0;

            CurrentState.Enter();
            m_CurrentStateTimeWatch = Stopwatch.StartNew();
        }
    }
}
