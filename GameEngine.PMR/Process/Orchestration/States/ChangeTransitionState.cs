using GameEngine.Core.FSM;
using GameEngine.PMR.Process.Transitions;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the ChangeTransition state of the Orchestrator, in which it starts a new transition and ends the previous ones
    /// </summary>
    internal class ChangeTransitionState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.ChangeTransition;

        private Orchestrator m_Orchestrator;

        private bool m_EntryCompleted;
        private bool m_ExitCompleted;
        private Transition m_PastTransition;

        internal ChangeTransitionState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            m_EntryCompleted = m_Orchestrator.CurrentTransition == null || m_Orchestrator.CurrentTransition.State == TransitionState.Running;
            m_ExitCompleted = !TryGetPastTransition(out m_PastTransition);
        }

        public override void Update()
        {
            if (!m_EntryCompleted)
            {
                m_EntryCompleted = EnterTransition();
            }
            else if (!m_ExitCompleted)
            {
                bool complete = ExitTransition();
                if (complete)
                {
                    m_PastTransition.BaseCleanup();
                    m_Orchestrator.PastTransitions.Dequeue();
                    m_ExitCompleted = !TryGetPastTransition(out m_PastTransition);
                }
            }

            if (m_EntryCompleted && m_ExitCompleted)
                SetState(OrchestratorState.RunTransition);
        }

        public override void Exit()
        {
            
        }

        private bool EnterTransition()
        {
            if (m_Orchestrator.CurrentTransition.State == TransitionState.Inactive && m_Orchestrator.CurrentTransition.IsReady)
                m_Orchestrator.CurrentTransition.BaseEnter();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Entering)
                m_Orchestrator.CurrentTransition.BaseUpdate();

            return m_Orchestrator.CurrentTransition.State == TransitionState.Running;
        }

        private bool ExitTransition()
        {
            if (m_PastTransition.State == TransitionState.Running)
                m_PastTransition.BaseExit();

            if (m_PastTransition.State == TransitionState.Exiting)
                m_PastTransition.BaseUpdate();

            return m_PastTransition.State == TransitionState.Inactive;
        }

        private bool TryGetPastTransition(out Transition transition)
        {
            transition = null;
            while (m_Orchestrator.PastTransitions.Count > 0)
            {
                transition = m_Orchestrator.PastTransitions.Peek();
                if (transition != null && transition != m_Orchestrator.CurrentTransition)
                    return true;
                    
                m_Orchestrator.PastTransitions.Dequeue();
            }

            return false;
        }
    }
}
