using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Scheduling;
using GameEngine.PJR.Process.Services;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyServiceSetup : IServiceSetup
    {
        public string Name => "Test";

        public void SetRules(ref RulesDictionary rules)
        {
            rules.AddRule(new DummyGameService());
        }

        public List<Type> GetInitUnloadOrder()
        {
            return new List<Type>()
            {
                typeof(DummyGameService)
            };
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            return new List<RuleScheduling>()
            {
                new RuleScheduling(typeof(DummyGameService), SchedulePattern.Default)
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
