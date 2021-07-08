using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;

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

    [RuleAccess(typeof(IStubGameRuleBis))]
    public class StubGameRuleBis : StubGameRule, IStubGameRuleBis
    {
    }

    public class StubGameRuleTer : StubGameRule
    {
        [RuleDependency(RuleDependencySource.Service, false)]
        public IStubGameService StubServiceReference;

        [RuleDependency(RuleDependencySource.SameModule, true)]
        public IStubGameRuleBis StubRuleBisReference;
    }

    [RuleAccess(typeof(IStubGameService))]
    public class StubGameService : StubGameRule, IStubGameService
    {

    }
}
