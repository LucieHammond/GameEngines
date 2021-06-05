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
    /// The FSM state corresponding to the UnloadRules state of the GameModule, in which GameRules are unloaded
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

        public UnloadRulesState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
            m_RuleUnloadTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameModule.Name, "Unload {0}", m_GameModule.IsService ? "services" : "rules");

            m_RulesToUnloadEnumerator = m_GameModule.Rules.GetRulesInReverseOrder(m_GameModule.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameModule.PerformancePolicy;
            m_SkipCurrrentRule = false;
            m_NbStallingWarnings = 0;
            if (!m_RulesToUnloadEnumerator.MoveNext())
                m_GameModule.GoToNextState();
        }

        public override void Update()
        {
            m_UpdateTime.Restart();
            do
            {
                if (m_RulesToUnloadEnumerator.Current.State == GameRuleState.Initialized)
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
                            break;
                    }
                }

                if (m_RulesToUnloadEnumerator.Current.State == GameRuleState.Unloaded ||
                    m_RulesToUnloadEnumerator.Current.State == GameRuleState.Unused ||
                    m_RulesToUnloadEnumerator.Current.State == GameRuleState.Initializing ||
                    m_SkipCurrrentRule)
                {
                    m_RuleUnloadTime.Stop();
                    m_SkipCurrrentRule = false;

                    if (!m_RulesToUnloadEnumerator.MoveNext())
                    {
                        m_GameModule.GoToNextState();
                        break;
                    }
                }
                else if (m_Performance.CheckStallingRules && m_RuleUnloadTime.ElapsedMilliseconds >= m_Performance.UnloadStallingTimeout)
                {
                    m_RuleUnloadTime.Restart();

                    if (m_NbStallingWarnings >= m_Performance.NbWarningsBeforeException)
                    {
                        int TotalTimeMs = m_Performance.UnloadStallingTimeout * (m_NbStallingWarnings + 1);
                        Exception e = new TimeoutException($"Rule unloading has been stalling for more than {TotalTimeMs}ms");
                        Log.Exception(m_RulesToUnloadEnumerator.Current.Name, e);
                        m_SkipCurrrentRule = m_GameModule.ExceptionPolicy.SkipUnloadIfException;
                        if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUnload))
                            break;
                    }
                    else
                    {
                        Log.Warning(m_RulesToUnloadEnumerator.Current.Name, $"Rule is pending for over {m_Performance.UnloadStallingTimeout} ms");
                        m_NbStallingWarnings++;
                    }
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            Log.Info(m_GameModule.Name, $"Unloading completed");
            m_RuleUnloadTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}
