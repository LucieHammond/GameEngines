using GameEngine.Core.FSM;
using GameEngine.PMR.Modules;

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
        private bool m_SubmodulesReset;
        private bool m_IsModuleReady;

        internal RunTransitionState(Orchestrator orchestrator)
        {
            m_Orchestrator = orchestrator;
        }

        public override void Enter()
        {
            m_SubmodulesReset = m_Orchestrator.Children.Count == 0;
            m_IsModuleReady = m_SubmodulesReset ? ExecuteNextOperation() : false;

            foreach (Orchestrator childOrchestrator in m_Orchestrator.Children)
            {
                if (childOrchestrator.CurrentModule != null)
                    childOrchestrator.UnloadModule();
            }
        }

        public override void Update()
        {
            m_Orchestrator.CurrentTransition?.BaseUpdate();

            if (!m_SubmodulesReset)
            {
                m_SubmodulesReset = PerformResetChildren();
                if (m_SubmodulesReset)
                    m_IsModuleReady = ExecuteNextOperation();
            }
            else if (!m_IsModuleReady)
            {
                bool complete = PerformModuleUpdate();
                if (complete)
                    m_IsModuleReady = ExecuteNextOperation();
            }

            if (m_IsModuleReady && m_Orchestrator.CurrentTransition?.IsComplete != false)
                SetState(OrchestratorState.ExitTransition);
        }

        public override void Exit()
        {

        }

        private bool PerformResetChildren()
        {
            foreach (Orchestrator childOrchestrator in m_Orchestrator.Children)
                childOrchestrator.Update();

            m_Orchestrator.Children.RemoveAll((orchestrator) => orchestrator.State == OrchestratorState.Wait);

            return m_Orchestrator.Children.Count == 0;
        }

        private bool PerformModuleUpdate()
        {
            if (m_Orchestrator.CurrentModule == null)
                return true;

            m_Orchestrator.CurrentModule.Update();

            if (m_Orchestrator.CurrentModule.State == GameModuleState.UpdateRules)
            {
                return true;
            }
            else if (m_Orchestrator.CurrentModule.State == GameModuleState.End)
            {
                m_Orchestrator.CurrentModule.Destroy();
                m_Orchestrator.CurrentModule = null;
                return true;
            }

            return false;
        }

        private bool ExecuteNextOperation()
        {
            if (m_Orchestrator.NextOperations.Count > 0)
            {
                m_Orchestrator.NextOperations.Dequeue().Invoke();
                return false;
            }

            return true;
        }
    }
}
