using GameEngine.Core.FSM;
using GameEngine.PMR.Process.Transitions;

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
                SetState(NextState());
                return;
            }
        }

        public override void Update()
        {
            if (m_Orchestrator.CurrentTransition.State == TransitionState.Running)
                m_Orchestrator.CurrentTransition.BaseExit();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Exiting)
                m_Orchestrator.CurrentTransition.BaseUpdate();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Inactive)
                SetState(NextState());

            if (m_Orchestrator.CurrentTransition.UpdateDuringExit)
                m_Orchestrator.CurrentModule?.Update();
        }

        public override void FixedUpdate()
        {
            if (m_Orchestrator.CurrentTransition.UpdateDuringExit)
                m_Orchestrator.CurrentModule?.FixedUpdate();
        }

        public override void LateUpdate()
        {
            if (m_Orchestrator.CurrentTransition.UpdateDuringExit)
                m_Orchestrator.CurrentModule?.LateUpdate();
        }

        public override void Exit()
        {

        }

        private OrchestratorState NextState()
        {
            if (m_Orchestrator.CurrentModule == null)
            {
                m_Orchestrator.CurrentTransition?.BaseCleanup();
                m_Orchestrator.CurrentTransition = null;
                
                return OrchestratorState.Wait;
            }
            else
            {
                return OrchestratorState.Operational;
            }
        }
    }
}
