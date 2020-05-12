using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Core.FSM.CustomFSM
{
    /// <summary>
    /// A state machine that transitions accross its states following a cycle pattern.
    /// Use this FSM if you need to visit the same states in the same order periodically.
    /// Those states will be represented in a list that can be modified using InsertStateInCycle and WithdrawStateFromCycle operations without effects on the FSM's current state.
    /// </summary>
    /// <typeparam name="T">An enum describing all possible states of this state machine.</typeparam>
    public class CycleFSM<T> : FSM<T> where T : Enum
    {
        private List<T> m_StateOrderedList;
        private int m_CurrentStateIndex;

        /// <summary>
        /// Constructor of the CycleFSM.
        /// </summary>
        /// <param name="name">The name of the CycleFSM.</param>
        /// <param name="states">An IEnumerable containing all the possible states of the CycleFSM.</param>
        /// <param name="cycleOrder">The ordered list of states to visit periodically.</param>
        public CycleFSM(string name, IEnumerable<FSMState<T>> states, List<T> cycleOrder) : base(name, states, cycleOrder[0])
        {
            foreach (T stateId in cycleOrder)
                CheckStateValidity(stateId);

            m_StateOrderedList = cycleOrder;
            m_CurrentStateIndex = 0;
        }

        /// <summary>
        /// Another constructor of the CycleFSM.
        /// </summary>
        /// <param name="name">The name of the CycleFSM.</param>
        /// <param name="states">A list of states representing all the possible states, ordered in the way they have to be visited periodically.</param>
        public CycleFSM(string name, List<FSMState<T>> states) : base(name, states, states[0].Id)
        {
            m_StateOrderedList = states.Select((state) => state.Id).ToList();
            m_CurrentStateIndex = 0;
        }

        /// <summary>
        /// Transition to the next state in the cycle.
        /// </summary>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <seealso cref="GameEngine.FSM.FSM.SetState"/>
        /// <returns>The id of the current state.</returns>
        public T MoveToNextState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            m_CurrentStateIndex++;
            m_CurrentStateIndex %= m_StateOrderedList.Count;
            SetState(m_StateOrderedList[m_CurrentStateIndex], immediate, ignoreIfCurrentState, priority);
            return m_StateOrderedList[m_CurrentStateIndex];
        }

        /// <summary>
        /// Insert a state at a specific index in the cycle.
        /// </summary>
        /// <param name="stateId">The id of the state to insert.</param>
        /// <param name="index">The index where to put the state in the ordered list representing the cycle.</param>
        public void InsertStateInCycle(T stateId, int index)
        {
            CheckStateValidity(stateId);
            m_StateOrderedList.Insert(index, stateId);

            if (m_CurrentStateIndex >= index)
                m_CurrentStateIndex++;
        }

        /// <summary>
        /// Withdraw a state from the cycle.
        /// </summary>
        /// <param name="index">The index of the state to remove in the ordered list representing the cycle.</param>
        public void WithdrawStateFromCycle(int index)
        {
            if (index == m_CurrentStateIndex)
                throw new InvalidOperationException($"Cannot withdraw the state at index {index} because the FSM is currently in that state.");
            
            m_StateOrderedList.RemoveAt(index);

            if (m_CurrentStateIndex > index)
                m_CurrentStateIndex--;
        }
    }
}
