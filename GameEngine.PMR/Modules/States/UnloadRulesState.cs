using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the UnloadRules state of the GameModule, in which the rules are unloaded
    /// </summary>
    internal class UnloadRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.UnloadRules;

        private GameModule m_GameModule;
        private IEnumerator<GameRule> m_RulesToUnloadEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleUnloadTime;
        private PerformancePolicy m_Performance;
        private bool m_SkipCurrrentRule;
        private int m_NbStallingWarnings;

        internal UnloadRulesState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
            m_RuleUnloadTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Unload rules");

            m_GameModule.ReportLoadingProgress(0f);

            m_Performance = m_GameModule.PerformancePolicy;
            m_SkipCurrrentRule = false;
            m_NbStallingWarnings = 0;
            m_RulesToUnloadEnumerator = m_GameModule.Rules.GetRulesInReverseOrder(m_GameModule.InitUnloadOrder).GetEnumerator();
            if (!m_RulesToUnloadEnumerator.MoveNext())
                m_RulesToUnloadEnumerator = null;
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
            m_RuleUnloadTime.Reset();
            m_UpdateTime.Reset();
        }

        private void TriggerActionForCurrentRule(out bool askExit)
        {
            askExit = false;

            if (m_RulesToUnloadEnumerator == null)
            {
                m_GameModule.GoToNextState();
                askExit = true;
            }
            else if (m_RulesToUnloadEnumerator.Current.State == GameRuleState.Initialized)
            {
                try
                {
                    m_RuleUnloadTime.Restart();
                    m_RulesToUnloadEnumerator.Current.BaseUnload();
                }
                catch (Exception e)
                {
                    Log.Exception(m_RulesToUnloadEnumerator.Current.Name, e);
                    m_SkipCurrrentRule = m_GameModule.ExceptionPolicy.SkipUnloadIfException;
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUnload))
                        askExit = true;
                }
            }
        }

        private void CheckResults(out bool askExit)
        {
            askExit = false;

            if (m_RulesToUnloadEnumerator.Current.State == GameRuleState.Unloaded ||
                m_RulesToUnloadEnumerator.Current.State == GameRuleState.Unused ||
                m_RulesToUnloadEnumerator.Current.State == GameRuleState.Initializing ||
                m_SkipCurrrentRule)
            {
                m_RuleUnloadTime.Stop();
                m_SkipCurrrentRule = false;
                m_NbStallingWarnings = 0;

                if (!m_RulesToUnloadEnumerator.MoveNext())
                    m_RulesToUnloadEnumerator = null;
            }
            else if (m_Performance.CheckStallingRules && m_RuleUnloadTime.ElapsedMilliseconds >= m_Performance.UnloadStallingTimeout)
            {
                m_RuleUnloadTime.Restart();
                m_NbStallingWarnings++;

                int totalStallingTime = m_Performance.UnloadStallingTimeout * m_NbStallingWarnings;
                if (m_NbStallingWarnings <= m_Performance.NbWarningsBeforeException)
                {
                    Log.Warning(GameModule.TAG, $"Unloading of rule {m_RulesToUnloadEnumerator.Current.Name} has been pending for {totalStallingTime} ms");
                }
                else
                {
                    Exception e = new TimeoutException($"The unloading of rule {m_RulesToUnloadEnumerator.Current.Name} is stalling (timeout = {totalStallingTime} ms)");
                    Log.Exception(GameModule.TAG, e);
                    m_SkipCurrrentRule = m_GameModule.ExceptionPolicy.SkipUnloadIfException;
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUnload))
                        askExit = true;
                }
            }
        }
    }
}
