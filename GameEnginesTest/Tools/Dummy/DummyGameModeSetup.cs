using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process.Modes;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameModeSetup : IGameModeSetup
    {
        public string Name => "Test";

        public Type RequiredServiceSetup => typeof(DummyServiceSetup);

        public void SetRules(ref RulesDictionary rules)
        {
            rules.AddRulePack(new DummyGameRulePack());
        }

        public List<Type> GetInitUnloadOrder()
        {
            return new List<Type>()
            {
                typeof(DummyGameRule),
                typeof(DummyGameRuleBis),
                typeof(DummyGameRuleTer)
            };
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            return new List<RuleScheduling>()
            {
                new RuleScheduling(typeof(DummyGameRuleBis), 2, 0),
                new RuleScheduling(typeof(DummyGameRule), SchedulePattern.Default),
                new RuleScheduling(typeof(DummyGameRuleTer), 2, 1)
            };
        }

        public ErrorPolicy GetErrorPolicy()
        {
            return new ErrorPolicy()
            {
                IgnoreExceptions = false,
                ReactionOnError = OnErrorBehaviour.PauseAll,
                SkipUnloadIfError = true,
                FallbackMode = null
            };
        }

        public PerformancePolicy GetPerformancePolicy()
        {
            return new PerformancePolicy()
            {
                MaxFrameDuration = 16,
                CheckStallingRules = false
            };
        }
    }
}
