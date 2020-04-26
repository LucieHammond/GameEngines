using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules.Dependencies.Attributes;

namespace GameEnginesTest.Tools.Dummy
{
    [DependencyProvider(typeof(IDummyGameService))]
    public class DummyGameService : GameService, IDummyGameService
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

    public interface IDummyGameService
    {

    }
}
