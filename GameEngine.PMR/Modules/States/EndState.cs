using GameEngine.Core.FSM;
using GameEngine.Core.Logger;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the End state of the GameModule, in which it does nothing except waiting to be stopped
    /// </summary>
    internal class EndState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.End;

        private GameModule m_GameModule;

        internal EndState(GameModule gameModule)
        {
            m_GameModule = gameModule;
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Finished");

            m_GameModule.ReportLoadingProgress(0f);
            m_GameModule.OnFinishUnloading?.Invoke();
            m_GameModule.OnFinishUnloading = null;
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }
}
