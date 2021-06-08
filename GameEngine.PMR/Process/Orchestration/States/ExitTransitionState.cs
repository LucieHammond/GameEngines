using GameEngine.Core.FSM;
using GameEngine.PMR.Modules.Transitions;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the ExitTransition state of the ModuleOrchestrator, in which it performs the exit phase of a transition
    /// </summary>
    internal class ExitTransitionState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.ExitTransition;

        private ModuleOrchestrator m_Orchestrator;

        internal ExitTransitionState(ModuleOrchestrator orchestrator)
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
            if (m_Orchestrator.CurrentModule == null)
            {
                m_Orchestrator.GoToState(ModuleOrchestratorState.Wait);
                m_Orchestrator.OnTerminated?.Invoke();
                m_Orchestrator.OnTerminated = null;
            }
            else if (m_Orchestrator.AwaitingAction != null)
            {
                m_Orchestrator.AwaitingAction();
                m_Orchestrator.AwaitingAction = null;
            }
            else
            {
                m_Orchestrator.GoToState(ModuleOrchestratorState.OperateModule);
            }
        }
    }
}
