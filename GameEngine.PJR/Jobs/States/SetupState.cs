using GameEngine.FSM;
using GameEngine.PJR.Process.Services;
using System;
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

        }

        public override void Update()
        {
            try
            {
                m_GameJob.ErrorPolicy = m_Setup.GetErrorPolicy();
                m_GameJob.PerformancePolicy = m_Setup.GetPerformancePolicy();

                m_Setup.SetRules(ref m_GameJob.Rules);
                if (m_GameJob.IsServiceJob)
                    (m_GameJob as IServiceSetup).CheckOnlyServices(m_GameJob.Rules);

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
            catch (Exception)
            {
                if (m_GameJob.ErrorPolicy == null || !m_GameJob.ErrorPolicy.IgnoreExceptions)
                {
                    m_GameJob.OnError();
                }
            }
        }

        public override void Exit()
        {

        }
    }

}
