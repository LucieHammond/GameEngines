using GameEngine.Core.FSM;
using GameEngine.PMR.Process.Transitions;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the EnterTransition state of the Orchestrator, in which it performs the entry phase of a transition
    /// </summary>
    internal class EnterTransitionState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.EnterTransition;

        private Orchestrator m_Orchestrator;

        internal EnterTransitionState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            if (m_Orchestrator.CurrentTransition == null)
            {
                SetState(OrchestratorState.RunTransition);
                return;
            }
        }

        public override void Update()
        {
            if (m_Orchestrator.CurrentTransition.State == TransitionState.Inactive && m_Orchestrator.CurrentTransition.IsReady)
                m_Orchestrator.CurrentTransition.BaseEnter();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Entering)
                m_Orchestrator.CurrentTransition.BaseUpdate();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Running)
                SetState(OrchestratorState.RunTransition);

            if (m_Orchestrator.CurrentTransition.UpdateDuringEntry)
                m_Orchestrator.CurrentModule?.Update();
        }

        public override void FixedUpdate()
        {
            if (m_Orchestrator.CurrentTransition.UpdateDuringEntry)
                m_Orchestrator.CurrentModule?.FixedUpdate();
        }

        public override void LateUpdate()
        {
            if (m_Orchestrator.CurrentTransition.UpdateDuringEntry)
                m_Orchestrator.CurrentModule?.LateUpdate();
        }

        public override void Exit()
        {

        }
    }
}
