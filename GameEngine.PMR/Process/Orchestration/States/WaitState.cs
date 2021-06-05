using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the Wait state of the ModuleOrchestrator, in which it does nothing but wait for its initialization or destruction
    /// </summary>
    internal class WaitState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.Wait;

        internal WaitState()
        {

        }

        public override void Enter()
        {

        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }
}
