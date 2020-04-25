using GameEngine.PSMR.Rules;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameRulesPack : IGameRulePack
    {
        public List<GameRule> CreatedRules { get; private set; }

        public IEnumerable<GameRule> GetRules()
        {
            CreatedRules = new List<GameRule>()
            {
                new DummyGameRule(),
                new DummyGameRuleBis(),
                new DummyGameRuleTer()
            };
            return CreatedRules;
        }
    }
}
