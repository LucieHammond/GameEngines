using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using System;
using System.Linq;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the Setup state of the GameModule, in which configurations are made using the IGameModuleSetup
    /// </summary>
    internal class SetupState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.Setup;

        private GameModule m_GameModule;
        private IGameModuleSetup m_Setup;

        internal SetupState(GameModule gameModule, IGameModuleSetup setup)
        {
            m_GameModule = gameModule;
            m_Setup = setup;
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Setup parameters");

            m_GameModule.ReportLoadingProgress(0f);
        }

        public override void Update()
        {
            try
            {
                m_Setup.SetRules(ref m_GameModule.Rules);

                m_GameModule.InitUnloadOrder = m_Setup.GetInitUnloadOrder().Where((ruleType) => m_GameModule.Rules.ContainsKey(ruleType)).ToList();
                m_GameModule.UpdateScheduler = m_Setup.GetUpdateScheduler().Where((scheduler) => m_GameModule.Rules.ContainsKey(scheduler.RuleType)).ToList();
                m_GameModule.FixedUpdateScheduler = m_Setup.GetFixedUpdateScheduler().Where((scheduler) => m_GameModule.Rules.ContainsKey(scheduler.RuleType)).ToList();
                m_GameModule.LateUpdateScheduler = m_Setup.GetLateUpdateScheduler().Where((scheduler) => m_GameModule.Rules.ContainsKey(scheduler.RuleType)).ToList();

                m_GameModule.ExceptionPolicy = m_Setup.GetExceptionPolicy();
                m_GameModule.PerformancePolicy = m_Setup.GetPerformancePolicy();

                CheckRulesOrderValidity();
                CheckExceptionPolicyValidity();
                CheckPerformancePolicyValidity();

                m_GameModule.GoToNextState();
            }
            catch (Exception e)
            {
                Log.Exception(m_GameModule.Name, e);
                if (m_GameModule.ExceptionPolicy != null)
                    m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad);
                else
                    m_GameModule.OnException(OnExceptionBehaviour.UnloadModule);
            }
        }

        public override void Exit()
        {

        }

        private void CheckRulesOrderValidity()
        {
            if (m_GameModule.InitUnloadOrder.GroupBy((type) => type).Any((group) => group.Count() > 1))
            {
                throw new Exception("InitUnloadOrder contains duplicated rules: each rule should be initialized and unloaded only once");
            }

            if (m_GameModule.UpdateScheduler.GroupBy((scheduler) => scheduler.RuleType).Any((group) => group.Count() > 1))
            {
                throw new Exception("UpdateScheduler contains duplicated rules: rules are not supposed to be updated more than once per frame");
            }

            if (m_GameModule.FixedUpdateScheduler.GroupBy((scheduler) => scheduler.RuleType).Any((group) => group.Count() > 1))
            {
                throw new Exception("FixedUpdateScheduler contains duplicated rules: rules are not supposed to be updated more than once per fixed frame");
            }

            if (m_GameModule.LateUpdateScheduler.GroupBy((scheduler) => scheduler.RuleType).Any((group) => group.Count() > 1))
            {
                throw new Exception("LateUpdateScheduler contains duplicated rules: rules are not supposed to be updated more than once per late frame");
            }
        }

        private void CheckExceptionPolicyValidity()
        {
            if (m_GameModule.ExceptionPolicy.ReactionDuringLoad == OnExceptionBehaviour.SwitchToFallback
                || m_GameModule.ExceptionPolicy.ReactionDuringUpdate == OnExceptionBehaviour.SwitchToFallback
                || m_GameModule.ExceptionPolicy.ReactionDuringUnload == OnExceptionBehaviour.SwitchToFallback)
            {
                if (m_GameModule.ExceptionPolicy.FallbackModule == null)
                    throw new Exception("FallbackModule cannot be null if one of the ExceptionBehaviour is set to SwitchToFallback");
            }
        }

        private void CheckPerformancePolicyValidity()
        {
            if (m_GameModule.PerformancePolicy.MaxFrameDuration <= 0)
                throw new Exception("MaxFrameDuration cannot be inferior or equal to zero");

            if (m_GameModule.PerformancePolicy.CheckStallingRules && m_GameModule.PerformancePolicy.InitStallingTimeout <= 0)
                throw new Exception("InitStallingTimeout cannot be inferior or equal to zero if CheckStallingRules is true");

            if (m_GameModule.PerformancePolicy.CheckStallingRules && m_GameModule.PerformancePolicy.UpdateStallingTimeout <= 0)
                throw new Exception("UpdateStallingTimeout cannot be inferior or equal to zero if CheckStallingRules is true");

            if (m_GameModule.PerformancePolicy.CheckStallingRules && m_GameModule.PerformancePolicy.UnloadStallingTimeout <= 0)
                throw new Exception("UnloadStallingTimeout cannot be inferior or equal to zero if CheckStallingRules is true");
        }
    }
}
