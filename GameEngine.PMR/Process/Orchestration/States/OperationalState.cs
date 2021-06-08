using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the Operational state of the Orchestrator, in which it executes the module and its submodules 
    /// without hiding them behind a transition
    /// </summary>
    internal class OperationalState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.Operational;

        private Orchestrator m_Orchestrator;

        internal OperationalState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            m_Orchestrator.OnOperational?.Invoke();
            m_Orchestrator.OnOperational = null;
        }

        public override void Update()
        {
            m_Orchestrator.CurrentModule.Update();

            foreach (Orchestrator childOrchestrator in m_Orchestrator.Children)
            {
                childOrchestrator.Update();
            }

            m_Orchestrator.Children.RemoveAll((orchestrator) => orchestrator.State == OrchestratorState.Wait);
        }

        public override void Exit()
        {

        }
    }
}
