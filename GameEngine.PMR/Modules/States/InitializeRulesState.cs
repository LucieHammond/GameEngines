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
    /// The FSM state corresponding to the InitializeRules state of the GameModule, in which GameRules are initialized
    /// </summary>
    internal class InitializeRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.InitializeRules;

        private GameModule m_GameModule;
        private IEnumerator<GameRule> m_RulesToInitEnumerator;
        private Stopwatch m_UpdateTime;
        private Stopwatch m_RuleInitTime;
        private PerformancePolicy m_Performance;
        private int m_NbRulesInitialized;
        private int m_NbStallingWarnings;

        public InitializeRulesState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_UpdateTime = new Stopwatch();
            m_RuleInitTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameModule.Name, "Initialize {0}", m_GameModule.IsService ? "services" : "rules");

            m_GameModule.LoadingProgress = 0;
            m_RulesToInitEnumerator = m_GameModule.Rules.GetRulesInOrder(m_GameModule.InitUnloadOrder).GetEnumerator();
            m_Performance = m_GameModule.PerformancePolicy;
            m_NbRulesInitialized = 0;
            m_NbStallingWarnings = 0;
            if (!m_RulesToInitEnumerator.MoveNext())
            {
                m_GameModule.LoadingProgress = 1;
                m_GameModule.GoToNextState();
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
                        if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                            break;
                    }
                }

                if (m_RulesToInitEnumerator.Current.ErrorDetected)
                {
                    m_GameModule.AskUnload();
                    break;
                }
                else if (m_RulesToInitEnumerator.Current.State == GameRuleState.Initialized)
                {
                    m_RuleInitTime.Stop();
                    m_NbRulesInitialized++;
                    m_GameModule.LoadingProgress = m_NbRulesInitialized / (float)m_GameModule.InitUnloadOrder.Count;

                    if (!m_RulesToInitEnumerator.MoveNext())
                    {
                        m_GameModule.LoadingProgress = 1;
                        m_GameModule.GoToNextState();
                        break;
                    }
                }
                else if (m_Performance.CheckStallingRules && m_RuleInitTime.ElapsedMilliseconds >= m_Performance.InitStallingTimeout)
                {
                    m_RuleInitTime.Restart();

                    if (m_NbStallingWarnings >= m_Performance.NbWarningsBeforeException)
                    {
                        int TotalTimeMs = m_Performance.InitStallingTimeout * (m_NbStallingWarnings + 1);
                        Exception e = new TimeoutException($"Rule initialization has been stalling for more than {TotalTimeMs} ms");
                        Log.Exception(m_RulesToInitEnumerator.Current.Name, e);
                        if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad))
                            break;
                    }
                    else
                    {
                        Log.Warning(m_RulesToInitEnumerator.Current.Name, $"Rule is pending for over {m_Performance.InitStallingTimeout} ms");
                        m_NbStallingWarnings++;
                    }
                }
            }
            while (m_UpdateTime.ElapsedMilliseconds < m_Performance.MaxFrameDuration);

            m_UpdateTime.Stop();
        }

        public override void Exit()
        {
            Log.Info(m_GameModule.Name, $"Initialization completed");
            m_RuleInitTime.Reset();
            m_UpdateTime.Reset();
        }
    }
}
