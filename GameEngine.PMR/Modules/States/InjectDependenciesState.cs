using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Process;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Collections.Generic;
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
        private IEnumerator<KeyValuePair<Type, GameRule>> m_RulesEnumerator;
        private Stopwatch m_UpdateTime;

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

            m_GameModule.ReportLoadingProgress(0f);

            m_RulesEnumerator = m_GameModule.Rules.GetEnumerator();
        }

        public override void Update()
        {
            if (m_GameModule.DependencyProvider == null)
            {
                try
                {
                    m_GameModule.DependencyProvider = RuleDependencyOperations.ExtractDependencies(m_GameModule.Rules);
                    if (m_ParentModule != null)
                        m_GameModule.DependencyProvider.LinkToParentProvider(m_ParentModule.DependencyProvider);
                }
                catch (Exception e)
                {
                    Log.Exception(m_GameModule.Name, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                        return;
                }
            }

            m_UpdateTime.Restart();

            do
            {
                if (!m_RulesEnumerator.MoveNext())
                {
                    m_GameModule.GoToNextState();
                    break;
                }

                GameRule rule = m_RulesEnumerator.Current.Value;
                DependencyProvider serviceProvider = m_MainProcess.ServiceProvider;
                DependencyProvider ruleProvider = m_GameModule.DependencyProvider;
                    
                try
                {
                    rule.InjectProcessDependencies(m_MainProcess, m_GameModule);
                    rule.InjectRuleDependencies(serviceProvider, ruleProvider);
                }
                catch (Exception e)
                {
                    Log.Exception(m_GameModule.Name, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                        break;
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_GameModule.PerformancePolicy.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_UpdateTime.Reset();
        }
    }
}