using GameEngine.Core.FSM;
using GameEngine.PMR.Modules.Transitions;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the ExitTransition state of the Orchestrator, in which it performs the exit phase of a transition
    /// </summary>
    internal class ExitTransitionState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.ExitTransition;

        private Orchestrator m_Orchestrator;

        internal ExitTransitionState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            if (m_Orchestrator.CurrentTransition == null)
            {
                GoToNextState();
                return;
            }

            m_Orchestrator.CurrentTransition.BaseStop();
        }

        public override void Update()
        {
            m_Orchestrator.CurrentTransition.BaseUpdate();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Inactive)
                GoToNextState();
        }

        public override void Exit()
        {

        }

        private void GoToNextState()
        {
            if (m_Orchestrator.AwaitingAction != null)
            {
                m_Orchestrator.AwaitingAction();
                m_Orchestrator.AwaitingAction = null;
            }
            else if (m_Orchestrator.CurrentModule == null)
            {
                SetState(OrchestratorState.Wait);
                m_Orchestrator.CurrentTransition?.BaseCleanup();
                m_Orchestrator.CurrentTransition = null;

                m_Orchestrator.OnTerminated?.Invoke();
                m_Orchestrator.OnTerminated = null;
            }
            else
            {
                SetState(OrchestratorState.Operational);
            }
        }
    }
}
