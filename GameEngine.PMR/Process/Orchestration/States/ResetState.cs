using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the Reset state of the Orchestrator, in which it unloads all the submodules in preparation for a future operation
    /// </summary>
    internal class ResetState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.Reset;

        private Orchestrator m_Orchestrator;

        internal ResetState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            foreach (Orchestrator childOrchestrator in m_Orchestrator.Children)
            {
                childOrchestrator.UnloadModule();
            }
        }

        public override void Update()
        {
            m_Orchestrator.CurrentModule.Update();

            foreach (Orchestrator childOrchestrator in m_Orchestrator.Children)
            {
                childOrchestrator.Update();
            }

            m_Orchestrator.Children.RemoveAll((orchestrator) => orchestrator.State == OrchestratorState.Wait);

            if (m_Orchestrator.Children.Count == 0)
                m_Orchestrator.OnReset();
        }

        public override void Exit()
        {
            m_Orchestrator.OnReset = null;
        }
    }
}
