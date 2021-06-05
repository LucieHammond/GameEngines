using GameEngine.Core.FSM;

namespace GameEngine.PMR.Modules.States
{
    /// <summary>
    /// The FSM state corresponding to the End state of the GameJob, in which it does nothing except waiting to be closed
    /// </summary>
    internal class EndState : FSMState<GameModuleState>
    {
        public override GameModuleState Id => GameModuleState.End;

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Update()
        {

        }
    }
}
