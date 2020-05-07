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
    /// The FSM state corresponding to the InitializeRules state of the GameJob, in which GameRules are initialized
    /// </summary>
    internal class InitializeRulesState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.InitializeRules;

        private GameJob m_GameJob;
        private IEnumerator<GameRule> m_RulesToInitEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleInitTime;
        private PerformancePolicy m_Performance;
        private int m_NbRulesInitialized;

        public InitializeRulesState(GameJob gameMode)
        {
            m_GameJob = gameMode;
            m_UpdateTime = new Stopwatch();
            m_RuleInitTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameJob.Name, "Initialize {0}", m_GameJob.IsServiceJob ? "services" : "rules");

            m_GameJob.LoadingProgress = 0;
            m_RulesToInitEnumerator = m_GameJob.Rules.GetRulesInOrder(m_GameJob.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameJob.PerformancePolicy;
            m_NbRulesInitialized = 0;
            if (!m_RulesToInitEnumerator.MoveNext())
            {
                m_GameJob.LoadingProgress = 1;
                m_GameJob.GoToNextState();
            }
        }

        public override void Update()
        {
            m_UpdateTime.Restart();
            do
            {
                if (m_RulesToInitEnumerator.Current.State == GameRuleState.Unused)
                {
                    try
                    {
                        m_RuleInitTime.Restart();
                        m_RulesToInitEnumerator.Current.BaseInitialize();
                    }
                    catch (Exception e)
                    {
                        Log.Exception(m_RulesToInitEnumerator.Current.Name, e);
                        if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringLoad))
                            break;
                    }
                }

                if (m_RulesToInitEnumerator.Current.ErrorDetected)
                {
                    m_GameJob.AskUnload();
                    break;
                }
                else if (m_RulesToInitEnumerator.Current.State == GameRuleState.Initialized)
                {
                    m_RuleInitTime.Stop();
                    m_NbRulesInitialized++;
                    m_GameJob.LoadingProgress = m_NbRulesInitialized / (float)m_GameJob.InitUnloadOrder.Count;

                    if (!m_RulesToInitEnumerator.MoveNext())
                    {
                        m_GameJob.LoadingProgress = 1;
                        m_GameJob.GoToNextState();
                        break;
                    }
                }
                else if (m_Performance.CheckStallingRules && m_RuleInitTime.ElapsedMilliseconds >= m_Performance.InitStallingTimeout)
                {
                    Exception e = new TimeoutException($"Rule initialization has been stalling for more than {m_Performance.InitStallingTimeout}ms");
                    Log.Exception(m_RulesToInitEnumerator.Current.Name, e);
                    
                    m_RuleInitTime.Restart();
                    if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringLoad))
                        break;
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            Log.Info(m_GameJob.Name, $"Initialization completed");
            m_RuleInitTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}
