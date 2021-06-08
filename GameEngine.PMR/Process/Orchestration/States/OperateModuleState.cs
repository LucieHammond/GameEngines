using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the OperateModule state of the ModuleOrchestrator, in which it executes the module and its submodules 
    /// without hiding them behind a transition
    /// </summary>
    internal class OperateModuleState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.OperateModule;

        private ModuleOrchestrator m_Orchestrator;

        internal OperateModuleState(ModuleOrchestrator orchestrator)
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
            m_Orchestrator.CurrentModule.InnerUpdate();

            foreach (ModuleOrchestrator submodule in m_Orchestrator.SubModules)
            {
                submodule.Update();
            }

            m_Orchestrator.SubModules.RemoveAll((orchestrator) => orchestrator.State == ModuleOrchestratorState.Wait);
        }

        public override void Exit()
        {

        }
    }
}
