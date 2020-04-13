using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.FSM.CustomFSM
{
    public class QueueFSM<T> : FSM<T> where T : Enum
    {
        private Queue<T> m_StateQueue;

        public QueueFSM(string name, IEnumerable<FSMState<T>> states, Queue<T> initialStateQueue) : base(name, states, initialStateQueue.Dequeue())
        {
            m_StateQueue = initialStateQueue;
        }

        public QueueFSM(string name, List<FSMState<T>> states) : base(name, states, states[0].Id)
        {
            for(int i = 1; i < states.Count; i++)
            {
                m_StateQueue.Enqueue(states[i].Id);
            } 
        }

        public void EnqueueState(T stateId)
        {
            CheckStateValidity(stateId);
            m_StateQueue.Enqueue(stateId);
        }

        public T DequeueState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            T stateId = m_StateQueue.Dequeue();
            SetState(stateId, immediate, ignoreIfCurrentState, priority);

            return stateId;
        }

        public bool TryDequeueState(out T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            if (m_StateQueue.TryDequeue(out stateId))
            {
                SetState(stateId, immediate, ignoreIfCurrentState, priority);
                return true;
            }

            return false;
        }
    }
}
