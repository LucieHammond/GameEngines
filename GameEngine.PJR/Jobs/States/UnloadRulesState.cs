using GameEngine.Core.Logger;
using GameEngine.FSM;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PJR.Jobs.States
{
    /// <summary>
    /// The FSM state corresponding to the UnloadRules state of the GameJob, in which GameRules are unloaded
    /// </summary>
    internal class UnloadRulesState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.UnloadRules;

        private GameJob m_GameJob;
        private IEnumerator<GameRule> m_RulesToUnloadEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleUnloadTime;
        private PerformancePolicy m_Performance;
        private bool m_SkipCurrrentRule;
        private int m_NbStallingWarnings;

        public UnloadRulesState(GameJob gameMode)
        {
            m_GameJob = gameMode;
            m_UpdateTime = new Stopwatch();
            m_RuleUnloadTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameJob.Name, "Unload {0}", m_GameJob.IsServiceJob ? "services" : "rules");

            m_RulesToUnloadEnumerator = m_GameJob.Rules.GetRulesInReverseOrder(m_GameJob.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameJob.PerformancePolicy;
            m_SkipCurrrentRule = false;
            m_NbStallingWarnings = 0;
            if (!m_RulesToUnloadEnumerator.MoveNext())
                m_GameJob.GoToNextState();
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
                        m_SkipCurrrentRule = m_GameJob.ExceptionPolicy.SkipUnloadIfException;
                        if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringUnload))
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
                        m_GameJob.GoToNextState();
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
                        m_SkipCurrrentRule = m_GameJob.ExceptionPolicy.SkipUnloadIfException;
                        if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringUnload))
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
            Log.Info(m_GameJob.Name, $"Unloading completed");
            m_RuleUnloadTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}
