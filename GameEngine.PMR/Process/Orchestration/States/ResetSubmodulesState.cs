using GameEngine.Core.FSM;

namespace GameEngine.PMR.Process.Orchestration.States
{
    /// <summary>
    /// The FSM state corresponding to the ResetSubmodules state of the ModuleOrchestrator, in which it unloads all the submodules in preparation 
    /// for a future module transition
    /// </summary>
    internal class ResetSubmodulesState : FSMState<ModuleOrchestratorState>
    {
        public override ModuleOrchestratorState Id => ModuleOrchestratorState.ResetSubmodules;

        private ModuleOrchestrator m_Orchestrator;

        internal ResetSubmodulesState(ModuleOrchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            foreach (ModuleOrchestrator submodule in m_Orchestrator.SubModules)
            {
                submodule.UnloadModule();
            }
        }

        public override void Update()
        {
            m_Orchestrator.CurrentModule.InnerUpdate();

            foreach (ModuleOrchestrator submodule in m_Orchestrator.SubModules)
            {
                submodule.Update();
            }

            m_Orchestrator.SubModules.RemoveAll((orchestrator) => orchestrator.State == ModuleOrchestratorState.Wait);

            if (m_Orchestrator.SubModules.Count == 0)
                m_Orchestrator.OnReset();
        }

        public override void Exit()
        {
            m_Orchestrator.OnReset = null;
        }
    }
}
