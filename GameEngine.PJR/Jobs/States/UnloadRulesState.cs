﻿using GameEngine.FSM;
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

        public UnloadRulesState(GameJob gameMode)
        {
            m_GameJob = gameMode;
            m_UpdateTime = new Stopwatch();
            m_RuleUnloadTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_RulesToUnloadEnumerator = m_GameJob.Rules.GetRulesInReverseOrder(m_GameJob.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameJob.PerformancePolicy;
            m_SkipCurrrentRule = false;
            if (!m_RulesToUnloadEnumerator.MoveNext())
                m_GameJob.GoToNextState();
        }

        public override void Update()
        {
            m_UpdateTime.Restart();
            do
            {
                bool blockingException = false;

                if (m_RulesToUnloadEnumerator.Current.State == GameRuleState.Initialized)
                {
                    try
                    {
                        m_RuleUnloadTime.Restart();
                        m_RulesToUnloadEnumerator.Current.BaseUnload();
                    }
                    catch (Exception)
                    {
                        if (!m_GameJob.ErrorPolicy.IgnoreExceptions)
                            blockingException = true;
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
                    Exception e = new TimeoutException($"The unloading of rule {m_RulesToUnloadEnumerator.Current.Name} has been stalling for more than {m_Performance.UnloadStallingTimeout}ms");
                    blockingException = true;
                    m_RuleUnloadTime.Restart();
                }

                if (blockingException || m_RulesToUnloadEnumerator.Current.ErrorDetected)
                {
                    if (m_GameJob.ErrorPolicy.SkipUnloadIfError)
                    {
                        m_SkipCurrrentRule = true;
                    }
                    if (m_GameJob.OnError())
                        break;
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_RuleUnloadTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}