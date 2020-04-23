using GameEngine.FSM;
using GameEngine.PSMR.Modes.Policies;
using GameEngine.PSMR.Rules;
using System;
using System.Diagnostics;

namespace GameEngine.PSMR.Modes.States
{
    /// <summary>
    /// The FSM state corresponding to the UpdateRules state of the GameMode, in which GameRules are updated
    /// </summary>
    internal class UpdateRulesState : FSMState<GameModeState>
    {
        public override GameModeState Id => GameModeState.UpdateRules;

        private GameMode m_GameMode;
        private Stopwatch m_RuleUpdateTime;
        private PerformancePolicy m_Performance;
        private int m_FrameCount;

        public UpdateRulesState(GameMode gameMode)
        {
            m_GameMode = gameMode;
            m_RuleUpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_Performance = m_GameMode.PerformancePolicy;
            m_FrameCount = 0;
        }

        public override void Update()
        {
            foreach (GameRule rule in m_GameMode.Rules.GetRulesInOrderForFrame(m_GameMode.UpdateScheduler, m_FrameCount))
            {
                bool blockingException = false;
                try
                {
                    m_RuleUpdateTime.Restart();
                    rule.BaseUpdate();
                    m_RuleUpdateTime.Stop();
                }
                catch (Exception)
                {
                    if (!m_GameMode.ErrorPolicy.IgnoreExceptions)
                        blockingException = true;
                }

                if (m_Performance.CheckStallingRules && m_RuleUpdateTime.ElapsedMilliseconds >= m_Performance.UpdateStallingTimeout)
                {
                    Exception e = new TimeoutException($"The update of rule {rule.Name} has taken more more than {m_Performance.UpdateStallingTimeout}ms to execute");
                    blockingException = true;
                }

                if (blockingException || rule.ErrorDetected)
                {
                    if (m_GameMode.OnError())
                        break;
                }
            }
            m_FrameCount++;
        }

        public override void Exit()
        {
            m_RuleUpdateTime.Reset();
        }
    }
}
