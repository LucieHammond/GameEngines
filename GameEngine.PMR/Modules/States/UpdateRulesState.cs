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
    /// The FSM state corresponding to the UpdateRules state of the GameModule, in which GameRules are updated
    /// </summary>
    internal class UpdateRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.UpdateRules;

        private GameModule m_GameModule;
        private IProcessTime m_Time;
        private Stopwatch m_RuleUpdateTime;
        private PerformancePolicy m_Performance;

        public UpdateRulesState(GameModule gameModule)
        {
            m_GameModule = gameModule;
            m_RuleUpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Info(m_GameModule.ParentProcess.Name, "<< {0} ready >>", m_GameModule.IsService ? "Services are" : "Game mode is");

            m_Performance = m_GameModule.PerformancePolicy;
            m_Time = m_GameModule.ParentProcess.Time;
        }

        public override void Update()
        {
            foreach (GameRule rule in m_GameModule.Rules.GetRulesInOrderForFrame(m_GameModule.UpdateScheduler, m_Time.FrameCount))
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
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUpdate))
                        break;
                }

                if (rule.ErrorDetected)
                {
                    m_GameModule.AskUnload();
                    break;
                }
                else if (m_Performance.CheckStallingRules && m_RuleUpdateTime.ElapsedMilliseconds >= m_Performance.UpdateStallingTimeout)
                {
                    Exception e = new TimeoutException($"Rule update has taken more than {m_Performance.UpdateStallingTimeout}ms to execute");
                    Log.Exception(rule.Name, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUpdate))
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