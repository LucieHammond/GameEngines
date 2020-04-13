using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.FSM.CustomFSM
{
    public class CycleFSM<T> : FSM<T> where T : Enum
    {
        private List<T> m_StateOrderedList;
        private int m_CurrentStateIndex;

        public CycleFSM(string name, IEnumerable<FSMState<T>> states, List<T> cycleOrder) : base(name, states, cycleOrder[0])
        {
            foreach (T stateId in cycleOrder)
                CheckStateValidity(stateId);

            m_StateOrderedList = cycleOrder;
            m_CurrentStateIndex = 0;
        }

        public CycleFSM(string name, List<FSMState<T>> states) : base(name, states, states[0].Id)
        {
            m_StateOrderedList = states.Select((state) => state.Id).ToList();
            m_CurrentStateIndex = 0;
        }

        public void MoveToNextState(bool immediate = false, bool ignoreIfCurrentState = false, byte priority = 10)
        {
            m_CurrentStateIndex++;
            m_CurrentStateIndex %= m_StateOrderedList.Count;
            SetState(m_StateOrderedList[m_CurrentStateIndex], immediate, ignoreIfCurrentState, priority);
        }

        public void InsertStateInCycle(T stateId, int index)
        {
            CheckStateValidity(stateId);
            m_StateOrderedList.Insert(index, stateId);

            if (m_CurrentStateIndex >= index)
                m_CurrentStateIndex++;
        }

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
