using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies.Attributes;

namespace GameEnginesTest.Tools.Dummy
{
    [DependencyProvider(typeof(IDummyGameService))]
    public class DummyGameService : GameRule, IDummyGameService
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
