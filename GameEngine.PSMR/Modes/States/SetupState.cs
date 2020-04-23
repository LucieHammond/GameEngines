using GameEngine.FSM;
using System;
using System.Linq;

namespace GameEngine.PSMR.Modes.States
{
    /// <summary>
    /// The FSM state corresponding to the Setup state of the GameMode, in which essential configurations are made using the GameModeSetup
    /// </summary>
    internal class SetupState : FSMState<GameModeState>
    {
        public override GameModeState Id => GameModeState.Setup;

        private GameMode m_GameMode;
        private IGameModeSetup m_Setup;

        public SetupState(GameMode gameMode, IGameModeSetup setup)
        {
            m_GameMode = gameMode;
            m_Setup = setup;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            try
            {
                m_GameMode.ErrorPolicy = m_Setup.GetErrorPolicy();
                m_GameMode.PerformancePolicy = m_Setup.GetPerformancePolicy();
                m_Setup.SetRules(ref m_GameMode.Rules);
                m_GameMode.InitUnloadOrder = m_Setup.GetInitUnloadOrder().Where((ruleType) => m_GameMode.Rules.ContainsKey(ruleType)).ToList();
                m_GameMode.UpdateScheduler = m_Setup.GetUpdateScheduler().Where((scheduler) => m_GameMode.Rules.ContainsKey(scheduler.RuleType)).ToList();

                m_GameMode.GoToNextState();
            }
            catch (Exception)
            {
                if (m_GameMode.ErrorPolicy == null || !m_GameMode.ErrorPolicy.IgnoreExceptions)
                {
                    m_GameMode.OnError();
                }
            }
        }

        public override void Exit()
        {

        }
    }

}
