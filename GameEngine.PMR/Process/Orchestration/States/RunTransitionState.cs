using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the RunTransition state of the ModuleOrchestrator, in which it supports the preparation of the module 
    /// with the front display of a transition
    /// </summary>
    internal class RunTransitionState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.RunTransition;

        private ModuleOrchestrator m_Orchestrator;

        internal RunTransitionState(ModuleOrchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            m_Orchestrator.CurrentModule.InnerUpdate();

            m_Orchestrator.CurrentTransition?.BaseUpdate();
        }

        public override void Exit()
        {

        }
    }
}
