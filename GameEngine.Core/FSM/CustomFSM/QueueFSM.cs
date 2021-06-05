using System;
using System.Collections.Generic;

namespace GameEngine.Core.FSM.CustomFSM
{
    /// <summary>
    /// A state machine that transitions accross its states following a queue pattern.
    /// Use this FSM if you can predict the order in which the states will be visited and this order only need to be followed in one direction.
    /// The states to cross are represented by a queue such as the first state of the queue will be the next state for the FSM to enter.
    /// The queue is modified using EnqueueState and DequeueState operations but only DequeueState will result in a change of state for the FSM.
    /// </summary>
    /// <typeparam name="T">An enum describing all possible states of this state machine.</typeparam>
    public class QueueFSM<T> : FSM<T> where T : Enum
    {
        private Queue<T> m_StateQueue;

        /// <summary>
        /// Constructor of the QueueFSM.
        /// </summary>
        /// <param name="name">The name of the QueueFSM.</param>
        /// <param name="states">An IEnumerable containing all the possible states of the QueueFSM.</param>
        /// <param name="initialStateQueue">A list of states used as initial queue.</param>
        public QueueFSM(string name, IEnumerable<FSMState<T>> states, List<T> initialStateQueue) : base(name, states, initialStateQueue[0])
        {
            initialStateQueue.RemoveAt(0);
            m_StateQueue = new Queue<T>(initialStateQueue);
        }

        /// <summary>
        /// Other constructor of the QueueFSM.
        /// </summary>
        /// <param name="name">The name of the QueueFSM.</param>
        /// <param name="states">A list of states representing all the possible states, ordered in the way they have to be visited.</param>
        public QueueFSM(string name, List<FSMState<T>> states) : base(name, states, states[0].Id)
        {
            m_StateQueue = new Queue<T>();
            for (int i = 1; i < states.Count; i++)
            {
                m_StateQueue.Enqueue(states[i].Id);
            }
        }

        /// <summary>
        /// Enqueue a state at the end of the queue, to be visited after the current last state.
        /// </summary>
        /// <param name="stateId">the id of the state to enqueue.</param>
        public void EnqueueState(T stateId)
        {
            CheckStateValidity(stateId);
            m_StateQueue.Enqueue(stateId);
        }

        /// <summary>
        /// Dequeue the first state of the queue in order to transition to that state.
        /// </summary>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <returns>The id of the dequeued state.</returns>
        public T DequeueState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            T stateId = m_StateQueue.Dequeue();
            SetState(stateId, immediate, ignoreIfCurrentState, priority);

            return stateId;
        }

        /// <summary>
        /// Try to dequeue the first state of the queue in order to transition to that state.
        /// </summary>
        /// <param name="stateId">out : the id of the dequeued state, if there was a state to dequeue (queue was not empty)</param>
        /// <param name="immediate">immediate argument used when setting new state</param>
        /// <param name="ignoreIfCurrentState">ignoreIfCurrentState argument used when setting new state</param>
        /// <param name="priority">priority argument used when setting new state</param>
        /// <returns>If the operation succeeded (if the queue wasn't empty)</returns>
        public bool TryDequeueState(out T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            if (m_StateQueue.Count > 0)
            {
                stateId = m_StateQueue.Dequeue();
                SetState(stateId, immediate, ignoreIfCurrentState, priority);
                return true;
            }

            stateId = default;
            return false;
        }

        /// <summary>
        /// Clear the queue of states
        /// </summary>
        public void ClearStateQueue()
        {
            m_StateQueue.Clear();
        }
    }
}
