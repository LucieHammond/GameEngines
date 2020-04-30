using GameEngine.FSM;

namespace GameEngine.PJR.Jobs.States
{
    /// <summary>
    /// The FSM state corresponding to the End state of the GameJob, in which it does nothing except waiting to be closed
    /// </summary>
    internal class EndState : FSMState<GameJobState>
    {
        public override GameJobState Id => GameJobState.End;

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
