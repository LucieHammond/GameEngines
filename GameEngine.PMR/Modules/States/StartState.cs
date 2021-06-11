using GameEngine.Core.FSM;
using GameEngine.Core.Logger;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the End state of the GameModule, in which it does nothing except waiting to be loaded
    /// </summary>
    internal class StartState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.Start;

        private GameModule m_GameModule;

        internal StartState(GameModule gameModule)
        {
            m_GameModule = gameModule;
        }

        public override void Enter()
        {
            Log.Debug(GameModule.TAG, $"{m_GameModule.Name}: Created");
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }
}
