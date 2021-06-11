using GameEngine.PMR.Rules;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubGameRulePack : IGameRulePack
    {
        public List<GameRule> CustomRules;

        public IEnumerable<GameRule> GetRules()
        {
            if (CustomRules != null)
                return CustomRules;

            return new List<GameRule>()
            {
                new StubGameRule(),
                new StubGameRuleBis(),
                new StubGameRuleTer()
            };
        }
    }
}
