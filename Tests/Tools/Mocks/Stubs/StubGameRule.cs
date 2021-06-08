using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using GameEngine.PMR.Rules.Dependencies.Attributes;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public interface IStubGameRuleBis { }
    public interface IStubGameService { }

    public class StubGameRule : GameRule
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

    [DependencyProvider(typeof(IStubGameRuleBis))]
    public class StubGameRuleBis : StubGameRule, IStubGameRuleBis
    {
    }

    public class StubGameRuleTer : StubGameRule
    {
        [DependencyConsumer(DependencyType.Service, false)]
        public IStubGameService StubServiceReference;

        [DependencyConsumer(DependencyType.Rule, true)]
        public IStubGameRuleBis StubRuleBisReference;
    }

    [DependencyProvider(typeof(IStubGameService))]
    public class StubGameService : StubGameRule, IStubGameService
    {

    }
}
