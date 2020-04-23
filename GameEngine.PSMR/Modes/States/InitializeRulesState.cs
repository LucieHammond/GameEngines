using GameEngine.FSM;
using GameEngine.PSMR.Modes.Policies;
using GameEngine.PSMR.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PSMR.Modes.States
{
    /// <summary>
    /// The FSM state corresponding to the InitializeRules state of the GameMode, in which GameRules are initialized
    /// </summary>
    internal class InitializeRulesState : FSMState<GameModeState>
    {
        public override GameModeState Id => GameModeState.InitializeRules;

        private GameMode m_GameMode;
        private IEnumerator<GameRule> m_RulesToInitEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleInitTime;
        private PerformancePolicy m_Performance;
        private int m_NbRulesInitialized;

        public InitializeRulesState(GameMode gameMode)
        {
            m_GameMode = gameMode;
            m_UpdateTime = new Stopwatch();
            m_RuleInitTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_GameMode.LoadingProgress = 0;
            m_RulesToInitEnumerator = m_GameMode.Rules.GetRulesInOrder(m_GameMode.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameMode.PerformancePolicy;
            m_NbRulesInitialized = 0;
            if (!m_RulesToInitEnumerator.MoveNext())
            {
                m_GameMode.LoadingProgress = 1;
                m_GameMode.GoToNextState();
            }
        }

        public override void Update()
        {
            m_UpdateTime.Restart();
            do
            {
                bool blockingException = false;
                if (m_RulesToInitEnumerator.Current.State == GameRuleState.Unused)
                {
                    try
                    {
                        m_RuleInitTime.Restart();
                        m_RulesToInitEnumerator.Current.BaseInitialize();
                    }
                    catch (Exception)
                    {
                        if (!m_GameMode.ErrorPolicy.IgnoreExceptions)
                            blockingException = true;
                    }
                }

                if (m_RulesToInitEnumerator.Current.State == GameRuleState.Initialized)
                {
                    m_RuleInitTime.Stop();
                    m_NbRulesInitialized++;
                    m_GameMode.LoadingProgress = m_NbRulesInitialized / (float)m_GameMode.InitUnloadOrder.Count;

                    if (!m_RulesToInitEnumerator.MoveNext())
                    {
                        m_GameMode.LoadingProgress = 1;
                        m_GameMode.GoToNextState();
                        break;
                    }
                }
                else if (m_Performance.CheckStallingRules && m_RuleInitTime.ElapsedMilliseconds >= m_Performance.InitStallingTimeout)
                {
                    Exception e = new TimeoutException($"The initialization of rule {m_RulesToInitEnumerator.Current.Name} has been stalling for more than {m_Performance.InitStallingTimeout}ms");
                    blockingException = true;
                    m_RuleInitTime.Restart();
                }

                if (blockingException || m_RulesToInitEnumerator.Current.ErrorDetected)
                {
                    if (m_GameMode.OnError())
                        break;
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            m_RuleInitTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}
