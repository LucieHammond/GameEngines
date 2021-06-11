using GameEngine.Core.FSM;
using GameEngine.PMR.Modules.Transitions;

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

            m_Orchestrator.CurrentTransition.BaseStart();
        }

        public override void Update()
        {
            m_Orchestrator.CurrentTransition.BaseUpdate();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Active)
                SetState(OrchestratorState.RunTransition);
        }

        public override void Exit()
        {

        }
    }
}
