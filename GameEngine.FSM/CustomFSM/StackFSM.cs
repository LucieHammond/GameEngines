using System;
using System.Collections.Generic;

namespace GameEngine.FSM.CustomFSM
{
    /// <summary>
    /// A state machine that transitions accross its states following a stack pattern.
    /// Use this FSM if you want to remember the states you passed in and come back to them later in reverse order.
    /// The states to remember are represented by a stack such as the state on top of the stack always corresponds to the last visited state (where to go back when the current is popped).
    /// The stack is modified using PushState and PopState operations that will result in a change of state for the FSM.
    /// </summary>
    /// <typeparam name="T">An enum describing all possible states of this state machine.</typeparam>
    public class StackFSM<T> : FSM<T> where T : Enum
    {
        private Stack<T> m_StatesStack;

        /// <summary>
        /// Constructor of the StackFSM. At first, the stack is empty : no previous state to remember.
        /// </summary>
        /// <param name="name">The name of the StackFSM.</param>
        /// <param name="states">An IEnumerable containing all the possible states of the StackFSM.</param>
        /// <param name="initialStateId">The first state to put on stack.</param>
        public StackFSM(string name, IEnumerable<FSMState<T>> states, T initialStateId) : base(name, states, initialStateId)
        {
            m_StatesStack = new Stack<T>();
        }

        /// <summary>
        /// Transition to a new state while pushing the current one on top of the stack to remember it.
        /// </summary>
        /// <param name="stateId">The id of the requested state (should be already part of FSM)</param>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <seealso cref="GameEngine.FSM.FSM.SetState"/>
        public void PushState(T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            m_StatesStack.Push(CurrentStateId);
            SetState(stateId, immediate, ignoreIfCurrentState, priority);
        }

        /// <summary>
        /// Pop the state placed on top of the stack in order to come back to it.
        /// </summary>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <seealso cref="GameEngine.FSM.FSM.SetState"/>
        /// <returns>The id of the withdrawn state</returns>
        public T PopState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            T stateId = m_StatesStack.Pop();
            SetState(stateId, immediate, ignoreIfCurrentState, priority);

            return stateId;
        }

        /// <summary>
        /// Try to pop the state placed on top of the stack in order to come back to it.
        /// </summary>
        /// <param name="stateId">out : the id of the withdrawn state, if there was a state to withdraw (stack was not empty)</param>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <seealso cref="GameEngine.FSM.FSM.SetState"/>
        /// <returns>If the operation succeeded (if the stack wasn't empty)</returns>
        public bool TryPopState(out T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            if (m_StatesStack.TryPop(out stateId))
            {
                SetState(stateId, immediate, ignoreIfCurrentState, priority);
                return true;
            }

            return false;
        }
    }
}
