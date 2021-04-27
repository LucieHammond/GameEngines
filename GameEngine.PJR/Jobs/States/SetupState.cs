﻿using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.PJR.Jobs.States
{
    /// <summary>
    /// The FSM state corresponding to the Setup state of the GameJob, in which essential configurations are made using the GameJobSetup
    /// </summary>
    internal class SetupState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.Setup;

        private GameJob m_GameJob;
        private IGameJobSetup m_Setup;

        public SetupState(GameJob gameMode, IGameJobSetup setup)
        {
            m_GameJob = gameMode;
            m_Setup = setup;
        }

        public override void Enter()
        {
            Log.Info(m_GameJob.Name, "Setup {0}", m_GameJob.IsServiceJob ? "service handler" : "game mode");
        }

        public override void Update()
        {
            try
            {
                m_GameJob.ExceptionPolicy = m_Setup.GetExceptionPolicy();
                m_GameJob.PerformancePolicy = m_Setup.GetPerformancePolicy();

                m_Setup.SetRules(ref m_GameJob.Rules);
                if (m_GameJob.IsServiceJob)
                    CheckOnlyServices(m_GameJob.Rules);

                m_GameJob.InitUnloadOrder = m_Setup.GetInitUnloadOrder().Where((ruleType) => m_GameJob.Rules.ContainsKey(ruleType)).ToList();
                m_GameJob.UpdateScheduler = m_Setup.GetUpdateScheduler().Where((scheduler) => m_GameJob.Rules.ContainsKey(scheduler.RuleType)).ToList();

                if (m_GameJob.InitUnloadOrder.GroupBy((type) => type).Any((group) => group.Count() > 1))
                {
                    throw new Exception("InitUnloadOrder contains duplicated rules : each rule should be initialized and unloaded only once");
                }

                if (m_GameJob.UpdateScheduler.GroupBy((scheduler) => scheduler.RuleType).Any((group) => group.Count() > 1))
                {
                    throw new Exception("UpdateScheduler contains duplicated rules : rules are not supposed to be updated more than once per frame");
                }

                m_GameJob.GoToNextState();
            }
            catch (Exception e)
            {
                Log.Exception(m_GameJob.Name, e);
                if (m_GameJob.ExceptionPolicy == null)
                    m_GameJob.OnException(OnExceptionBehaviour.UnloadJob);
                else
                    m_GameJob.OnException(m_GameJob.ExceptionPolicy.ReactionDuringLoad);
            }
        }

        public override void Exit()
        {
            Log.Info(m_GameJob.Name, $"Setup completed");
        }

        private void CheckOnlyServices(RulesDictionary rules)
        {
            foreach (KeyValuePair<Type, GameRule> rule in rules)
            {
                if (!(rule.Value is GameService))
                {
                    throw new InvalidCastException($"Cannot setup rule {rule.Value.Name} in ServiceSetup because it doesn't inherit GameService");
                }
            }
        }
    }

}
