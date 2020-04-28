using GameEngine.FSM;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process;
using GameEngine.PJR.Rules;
using System;
using System.Diagnostics;

namespace GameEngine.PJR.Jobs.States
{
    /// <summary>
    /// The FSM state corresponding to the UpdateRules state of the GameJob, in which GameRules are updated
    /// </summary>
    internal class UpdateRulesState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.UpdateRules;

        private GameJob m_GameJob;
        private IProcessTime m_Time;
        private Stopwatch m_RuleUpdateTime;
        private PerformancePolicy m_Performance;

        public UpdateRulesState(GameJob gameMode)
        {
            m_GameJob = gameMode;
            m_RuleUpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            m_Performance = m_GameJob.PerformancePolicy;
            m_Time = m_GameJob.ParentProcess.Time;
        }

        public override void Update()
        {
            foreach (GameRule rule in m_GameJob.Rules.GetRulesInOrderForFrame(m_GameJob.UpdateScheduler, m_Time.FrameCount))
            {
                try
                {
                    m_RuleUpdateTime.Restart();
                    rule.BaseUpdate();
                    m_RuleUpdateTime.Stop();
                }
                catch (Exception)
                {
                    if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringUpdate))
                        break;
                }

                if (rule.ErrorDetected)
                {
                    m_GameJob.AskUnload();
                    break;
                }
                else if (m_Performance.CheckStallingRules && m_RuleUpdateTime.ElapsedMilliseconds >= m_Performance.UpdateStallingTimeout)
                {
                    Exception e = new TimeoutException($"The update of rule {rule.Name} has taken more more than {m_Performance.UpdateStallingTimeout}ms to execute");
                    if (m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringUpdate))
                        break;
                }
            }
        }

        public override void Exit()
        {
            m_RuleUpdateTime.Reset();
        }
    }
}