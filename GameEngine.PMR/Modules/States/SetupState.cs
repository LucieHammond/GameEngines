using GameEngine.Core.FSM;
using GameEngine.Core.Logger;
using GameEngine.PMR.Modules.Policies;
using System;
using System.Linq;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the Setup state of the GameModule, in which essential configurations are made using the IGameModuleSetup
    /// </summary>
    internal class SetupState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.Setup;

        private GameModule m_GameModule;
        private IGameModuleSetup m_Setup;

        public SetupState(GameModule gameModule, IGameModuleSetup setup)
        {
            m_GameModule = gameModule;
            m_Setup = setup;
        }

        public override void Enter()
        {
            Log.Info(m_GameModule.Name, "Setup {0}", m_GameModule.IsService ? "service handler" : "game mode");
        }

        public override void Update()
        {
            try
            {
                m_GameModule.ExceptionPolicy = m_Setup.GetExceptionPolicy();
                m_GameModule.PerformancePolicy = m_Setup.GetPerformancePolicy();

                m_Setup.SetRules(ref m_GameModule.Rules);

                m_GameModule.InitUnloadOrder = m_Setup.GetInitUnloadOrder().Where((ruleType) => m_GameModule.Rules.ContainsKey(ruleType)).ToList();
                m_GameModule.UpdateScheduler = m_Setup.GetUpdateScheduler().Where((scheduler) => m_GameModule.Rules.ContainsKey(scheduler.RuleType)).ToList();

                if (m_GameModule.InitUnloadOrder.GroupBy((type) => type).Any((group) => group.Count() > 1))
                {
                    throw new Exception("InitUnloadOrder contains duplicated rules : each rule should be initialized and unloaded only once");
                }

                if (m_GameModule.UpdateScheduler.GroupBy((scheduler) => scheduler.RuleType).Any((group) => group.Count() > 1))
                {
                    throw new Exception("UpdateScheduler contains duplicated rules : rules are not supposed to be updated more than once per frame");
                }

                m_GameModule.GoToNextState();
            }
            catch (Exception e)
            {
                Log.Exception(m_GameModule.Name, e);
                if (m_GameModule.ExceptionPolicy == null)
                    m_GameModule.OnException(OnExceptionBehaviour.UnloadModule);
                else
                    m_GameModule.OnException(m_GameModule.ExceptionPolicy.ReactionDuringLoad);
            }
        }

        public override void Exit()
        {
            Log.Info(m_GameModule.Name, $"Setup completed");
        }
    }

}
