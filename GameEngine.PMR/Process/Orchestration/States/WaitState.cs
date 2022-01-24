using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the Wait state of the Orchestrator, in which it does nothing but wait for its initialization or destruction
    /// </summary>
    internal class WaitState : FSMState<OrchestratorState>
    {
        public override OrchestratorState Id => OrchestratorState.Wait;

        private Orchestrator m_Orchestrator;

        internal WaitState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            m_Orchestrator.OnTerminated?.Invoke();
            m_Orchestrator.OnTerminated = null;
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }
}
