using GameEngine.FSM;

namespace GameEngine.PSMR.Modes.States
{
    /// <summary>
    /// The FSM state corresponding to the End state of the GameMode, in which it does nothing except waiting to be closed
    /// </summary>
    internal class EndState : FSMState<GameModeState>
    {
        public override GameModeState Id => GameModeState.End;

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
