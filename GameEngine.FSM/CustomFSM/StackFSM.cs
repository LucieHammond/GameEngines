using System;
using System.Collections.Generic;

namespace GameEngine.FSM.CustomFSM
{
    public class StackFSM<T> : FSM<T> where T : Enum
    {
        private Stack<T> m_StatesStack;

        public StackFSM(string name, IEnumerable<FSMState<T>> states, T initialStateId) : base(name, states, initialStateId)
        {
            m_StatesStack = new Stack<T>();
            m_StatesStack.Push(initialStateId);
        }

        public void PushState(T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            m_StatesStack.Push(stateId);
            SetState(stateId, immediate, ignoreIfCurrentState, priority);
        }

        public T PopState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            T stateId = m_StatesStack.Pop();
            SetState(m_StatesStack.Peek(), immediate, ignoreIfCurrentState, priority);

            return stateId;
        }

        public bool TryPopState(out T stateId, bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            if (m_StatesStack.TryPop(out stateId))
            {
                SetState(m_StatesStack.Peek(), immediate, ignoreIfCurrentState, priority);
                return true;
            }

            return false;
        }
    }
}
