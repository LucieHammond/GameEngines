using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the UpdateRules state of the GameModule, in which the rules are updated
    /// </summary>
    internal class UpdateRulesState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.UpdateRules;

        private GameModule m_GameModule;
        private ITime m_Time;
        private Stopwatch m_RuleUpdateTime;
        private PerformancePolicy m_Performance;

        internal UpdateRulesState(GameModule gameModule, ITime time)
        {
            m_GameModule = gameModule;
            m_Time = time;
            m_RuleUpdateTime = new Stopwatch();
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Ready to update rules");

            m_GameModule.ReportLoadingProgress(1f);
            m_GameModule.OnFinishLoading?.Invoke();
            m_GameModule.OnFinishLoading = null;

            m_Performance = m_GameModule.PerformancePolicy;
        }

        public override void Update()
        {
            UpdateRules(m_GameModule.UpdateScheduler, (rule) => rule.BaseUpdate());
        }

        public override void FixedUpdate()
        {
            UpdateRules(m_GameModule.FixedUpdateScheduler, (rule) => rule.BaseFixedUpdate());
        }

        public override void LateUpdate()
        {
            UpdateRules(m_GameModule.LateUpdateScheduler, (rule) => rule.BaseLateUpdate());
        }

        public override void Exit()
        {
            m_RuleUpdateTime.Reset();
        }

        private void UpdateRules(List<RuleScheduling> updateScheduler, Action<GameRule> ruleUpdate)
        {
            foreach (GameRule rule in m_GameModule.Rules.GetRulesInOrderForFrame(updateScheduler, m_Time.FrameCount))
            {
                try
                {
                    m_RuleUpdateTime.Restart();
                    ruleUpdate(rule);
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
                    m_GameModule.OnManagedError();
                    break;
                }
                else if (m_Performance.CheckStallingRules && m_RuleUpdateTime.ElapsedMilliseconds >= m_Performance.UpdateStallingTimeout)
                {
                    int stallingTime = m_Performance.UpdateStallingTimeout;
                    Exception e = new TimeoutException($"The update of rule {rule.Name} has taken too much time (timeout = {stallingTime} ms)");
                    Log.Exception(GameModule.TAG, e);
                    if (m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringUpdate))
                        break;
                }
            }
        }
    }
}