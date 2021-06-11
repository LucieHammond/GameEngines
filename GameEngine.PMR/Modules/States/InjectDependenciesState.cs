using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the InjectDependencies state of the GameModule, in which the dependencies of the rules are filled
    /// </summary>
    internal class InjectDependenciesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.InjectDependencies;

        private GameModule m_GameModule;
        private GameProcess m_MainProcess;
        private GameModule m_ParentModule;
        private Stopwatch m_UpdateTime;
        private PerformancePolicy m_Performance;

        private bool m_IsProcessInjected;

        internal InjectDependenciesState(GameModule gameModule, GameProcess process, GameModule parent)
        {
            m_GameModule = gameModule;
            m_MainProcess = process;
            m_ParentModule = parent;
            m_UpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Inject dependencies");

            m_Performance = m_GameModule.PerformancePolicy;
            m_IsProcessInjected = false;

            m_GameModule.ReportLoadingProgress(0f);
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            try
            {
                if (!m_IsProcessInjected)
                {
                    foreach (GameRule rule in m_GameModule.Rules.Values)
                    {
                        rule.InjectProcessDependencies(m_MainProcess, m_GameModule);
                    }
                    m_IsProcessInjected = true;

                    if (m_UpdateTime.ElapsedMilliseconds >= m_Performance.MaxFrameDuration)
                        return;
                }

                if (m_GameModule.DependencyProvider == null)
                {
                    m_GameModule.DependencyProvider = DependencyUtils.ExtractDependencies(m_GameModule.Rules);
                    if (m_ParentModule != null)
                        m_GameModule.DependencyProvider.LinkToParentProvider(m_ParentModule.DependencyProvider);

                    if (m_UpdateTime.ElapsedMilliseconds >= m_Performance.MaxFrameDuration)
                        return;
                }

                DependencyProvider serviceProvider = m_MainProcess.ServiceProvider;
                DependencyProvider ruleProvider = m_GameModule.DependencyProvider;
                DependencyUtils.InjectDependencies(m_GameModule.Rules, serviceProvider, ruleProvider);

                m_GameModule.GoToNextState();
            }
            catch (Exception e)
            {
                Log.Exception(m_GameModule.Name, e);
                m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad);
            }

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_UpdateTime.Reset();
        }
    }
}