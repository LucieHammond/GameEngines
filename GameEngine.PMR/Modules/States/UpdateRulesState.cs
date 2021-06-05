using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process;
using GameEngine.PMR.Rules;
using System;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the UpdateRules state of the GameJob, in which GameRules are updated
    /// </summary>
    internal class UpdateRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.UpdateRules;

        private GameModule m_GameJob;
        private IProcessTime m_Time;
        private Stopwatch m_RuleUpdateTime;
        private PerformancePolicy m_Performance;

        public UpdateRulesState(GameModule gameMode)
        {
            m_GameJob = gameMode;
            m_RuleUpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameJob.ParentProcess.Name, "<< {0} ready >>", m_GameJob.IsServiceJob ? "Services are" : "Game mode is");

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
                catch (Exception e)
                {
                    Log.Exception(rule.Name, e);
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
                    Exception e = new TimeoutException($"Rule update has taken more than {m_Performance.UpdateStallingTimeout}ms to execute");
                    Log.Exception(rule.Name, e);
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