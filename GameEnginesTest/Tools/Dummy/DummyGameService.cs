using GameEngine.PJR.Process.Services;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameService : GameService
    {
        protected override void Initialize()
        {
            MarkInitialized();
        }

        protected override void Update()
        {

        }

        protected override void Unload()
        {
            MarkUnloaded();
        }
    }
}
