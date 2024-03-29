﻿using GameEngine.Core.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.Core.FSM
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
            get => m_CurrentStateTimeWatch != null ? m_CurrentStateTimeWatch.Elapsed.TotalSeconds : 0;
        }

        private bool m_Running;
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
            m_Running = false;

            foreach (FSMState<T> state in states)
            {
                AddState(state);
            }

            CheckStateValidity(initialStateId);
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
#endif
            foreach (KeyValuePair<T, FSMState<T>> stateEntry in m_States)
            {
                stateEntry.Value.Initialize();
            }

            CurrentState.Enter();
            m_CurrentStateTimeWatch = Stopwatch.StartNew();

            m_Running = true;
        }

        /// <summary>
        /// Update the FSM : update the current state and change state if requested
        /// </summary>
        public void Update()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before Update");
#endif
            while (m_StateChangeRequested)
            {
                SwitchToNextState();
            }

            CurrentState.Update();
        }

        /// <summary>
        /// Perform the fixed update of the FSM : call the fixed update of the current state
        /// </summary>
        public void FixedUpdate()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before FixedUpdate");
#endif
            CurrentState.FixedUpdate();
        }

        /// <summary>
        /// Perform the late update of the FSM : call the late update of the current state
        /// </summary>
        public void LateUpdate()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before LateUpdate");
#endif
            CurrentState.LateUpdate();
        }

        /// <summary>
        /// Stop the FSM : exit the current state and unload all states
        /// </summary>
        public void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before Stop");
#endif
            m_CurrentStateTimeWatch.Stop();
            CurrentState.Exit();

            foreach (KeyValuePair<T, FSMState<T>> stateEntry in m_States)
            {
                stateEntry.Value.Unload();
            }

            m_Running = false;
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
            CheckStateValidity(resetStateId);
            CurrentStateId = resetStateId;
        }

        /// <summary>
        /// Request a state change.
        /// </summary>
        /// <param name="stateId">The id of the requested state (enum value)</param>
        /// <param name="immediate">If the change should be applied immediatly or if it should wait the start of the next update. Default = false</param>
        /// <param name="ignoreIfCurrentState">If true, the method does nothing when the requested state is the same as the current state. If false, it exits and enters that same state. Default = false</param>
        /// <param name="priority">The priority of the state change request, between 0 (lowest priority) and 255 (highest priority). Used to decide between simultaneous requests. Default = 10</param>
        public void SetState(T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!m_Running)
                throw new InvalidOperationException($"The state machine should be started before changing state");
#endif
            CheckStateValidity(stateId);

            if (ignoreIfCurrentState && stateId.Equals(CurrentStateId))
                return;

            if (m_StateChangeRequested)
            {
                if (priority < m_StateChangePriority)
                    return;
                else if (priority == m_StateChangePriority)
                    Log.Warning("FSM", $"Concurrent state change requests were made with the same priority. Oldest call will be ignored.\n" +
                        $"Previous target state: {m_NextStateId}\n" + $"New target state: {stateId}");
            }

            m_StateChangeRequested = true;
            m_NextStateId = stateId;
            m_StateChangePriority = priority;

            if (immediate)
            {
                SwitchToNextState();
            }
        }

        /// <summary>
        /// Add a new state in the collection of states attached to the FSM. The FSM can now enter this state.
        /// </summary>
        /// <param name="state">The new FSM state.</param>
        public void AddState(FSMState<T> state)
        {
            if (m_States.ContainsKey(state.Id))
                throw new ArgumentException($"A state machine cannot have multiple states with same id {state.Id}", "states");

            m_States.Add(state.Id, state);
            state.AttachToFSM(this);

            if (m_Running)
                m_States[state.Id].Initialize();
        }

        /// <summary>
        /// Remove a state from the collection of states attached to the FSM. The FSM can no longer enter this state.
        /// </summary>
        /// <param name="stateId"></param>
        public void RemoveState(T stateId)
        {
            CheckStateValidity(stateId);

            if (stateId.Equals(CurrentStateId))
                throw new InvalidOperationException($"Cannot remove state {stateId} because the FSM is currently in that state.");

            if (m_Running)
                m_States[stateId].Unload();

            m_States.Remove(stateId);
        }

        /// <summary>
        /// Tell if a state is part of the FSM, that means if the FSM can enter that state.
        /// </summary>
        /// <param name="stateId">The id of the state to check</param>
        /// <returns>If the FSM contains that state</returns>
        public bool IsValidState(T stateId)
        {
            return m_States.ContainsKey(stateId);
        }

        /// <summary>
        /// Ensure that a given state is valid for this FSM, otherwise throw ArgumentException
        /// </summary>
        /// <param name="stateId">The id of the state to check</param>
        /// <exception cref="ArgumentException">Thrown if the state is invalid</exception>
        protected void CheckStateValidity(T stateId)
        {
            if (!IsValidState(stateId))
                throw new ArgumentException($"The state {stateId} is not a valid state");
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
