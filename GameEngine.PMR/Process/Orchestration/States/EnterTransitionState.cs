using GameEngine.Core.FSM;
using GameEngine.PMR.Modules.Transitions;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the EnterTransition state of the ModuleOrchestrator, in which it performs the entry phase of a transition
    /// </summary>
    internal class EnterTransitionState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.EnterTransition;

        private ModuleOrchestrator m_Orchestrator;

        internal EnterTransitionState(ModuleOrchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            if (m_Orchestrator.CurrentTransition == null)
                m_Orchestrator.GoToState(ModuleOrchestratorState.RunTransition);

            m_Orchestrator.CurrentTransition.BaseStart();
        }

        public override void Update()
        {
            m_Orchestrator.CurrentTransition.BaseUpdate();

            if (m_Orchestrator.CurrentTransition.State == TransitionState.Active)
                m_Orchestrator.GoToState(ModuleOrchestratorState.RunTransition);
        }

        public override void Exit()
        {

        }
    }
}
