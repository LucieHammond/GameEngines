using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the RunTransition state of the Orchestrator, in which it supports the preparation of the module while 
    /// displaying a transition
    /// </summary>
    internal class RunTransitionState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.RunTransition;

        private Orchestrator m_Orchestrator;

        internal RunTransitionState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            m_Orchestrator.CurrentModule.Update();

            m_Orchestrator.CurrentTransition?.BaseUpdate();
        }

        public override void Exit()
        {

        }
    }
}
