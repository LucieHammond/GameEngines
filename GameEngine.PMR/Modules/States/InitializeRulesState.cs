using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.Core.Utilities;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the InitializeRules state of the GameModule, in which the rules are initialized
    /// </summary>
    internal class InitializeRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.InitializeRules;

        private GameModule m_GameModule;
        private IEnumerator<GameRule> m_RulesToInitEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleInitTime;
        private PerformancePolicy m_Performance;
        private float m_InitialProgress;
        private int m_NbRulesInitialized;
        private int m_NbStallingWarnings;

        internal InitializeRulesState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
            m_RuleInitTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Initialize rules");

            m_InitialProgress = m_GameModule.SpecialTasks.Count / (m_GameModule.SpecialTasks.Count + 3);
            m_GameModule.ReportLoadingProgress(m_InitialProgress);

            m_Performance = m_GameModule.PerformancePolicy;
            m_NbRulesInitialized = 0;
            m_NbStallingWarnings = 0;
            m_RulesToInitEnumerator = m_GameModule.Rules.GetRulesInOrder(m_GameModule.InitUnloadOrder).GetEnumerator();
            if (!m_RulesToInitEnumerator.MoveNext())
                m_RulesToInitEnumerator = null;
        }

        public override void Update()
        {
            m_UpdateTime.Restart();

            do
            {
                TriggerActionForCurrentRule(out bool askExit);
                if (askExit)
                    break;

                CheckResults(out askExit);
                if (askExit)
                    break;
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_RuleInitTime.Reset();
            m_UpdateTime.Reset();
        }

        private void TriggerActionForCurrentRule(out bool askExit)
        {
            askExit = false;

            if (m_RulesToInitEnumerator == null)
            {
                m_GameModule.GoToNextState();
                askExit = true;
            }
            else if (m_RulesToInitEnumerator.Current.State == GameRuleState.Unused ||
                m_RulesToInitEnumerator.Current.State == GameRuleState.Unloaded)
            {
                try
                {
                    m_RuleInitTime.Restart();
                    m_RulesToInitEnumerator.Current.BaseInitialize();
                }
                catch (Exception e)
                {
                    Log.Exception(m_RulesToInitEnumerator.Current.Name, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                        askExit = true;
                }
            }
        }

        private void CheckResults(out bool askExit)
        {
            askExit = false;

            if (m_RulesToInitEnumerator.Current.State == GameRuleState.Initialized)
            {
                m_RuleInitTime.Stop();
                m_NbRulesInitialized++;
                m_NbStallingWarnings = 0;

                ReportProgress(m_NbRulesInitialized / (float)m_GameModule.InitUnloadOrder.Count);
                if (!m_RulesToInitEnumerator.MoveNext())
                    m_RulesToInitEnumerator = null;
            }
            else if (m_RulesToInitEnumerator.Current.ErrorDetected)
            {
                m_GameModule.OnManagedError();
                askExit = true;
            }
            else if (m_Performance.CheckStallingRules && m_RuleInitTime.ElapsedMilliseconds >= m_Performance.InitStallingTimeout)
            {
                m_RuleInitTime.Restart();
                m_NbStallingWarnings++;

                int totalStallingTime = m_Performance.InitStallingTimeout * m_NbStallingWarnings;
                if (m_NbStallingWarnings <= m_Performance.NbWarningsBeforeException)
                {
                    Log.Warning(GameModule.TAG, $"Initialization of rule {m_RulesToInitEnumerator.Current.Name} has been pending for {totalStallingTime} ms");
                }
                else
                {
                    Exception e = new TimeoutException($"The initialization of rule {m_RulesToInitEnumerator.Current.Name} is stalling (timeout = {totalStallingTime} ms)");
                    Log.Exception(GameModule.TAG, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                        askExit = true;
                }
            }
        }

        private void ReportProgress(float initProgress)
        {
            float totalProgress = MathUtils.Lerp(initProgress, m_InitialProgress, 1f);
            m_GameModule.ReportLoadingProgress(totalProgress);
        }
    }
}
