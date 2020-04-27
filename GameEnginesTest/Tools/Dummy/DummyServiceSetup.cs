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

        public List<GameService> CustomServices;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public ErrorPolicy CustomErrorPolicy;
        public PerformancePolicy CustomPerformancePolicy;

        public void SetRules(ref RulesDictionary rules)
        {
            if (CustomServices != null)
            {
                foreach (GameService service in CustomServices)
                    rules.AddRule(service);
            }
            else
            {
                rules.AddRule(new DummyGameService());
            }
        }

        public List<Type> GetInitUnloadOrder()
        {
            if (CustomInitUnloadOrder != null)
                return CustomInitUnloadOrder;

            return new List<Type>()
            {
                typeof(DummyGameService)
            };
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            if (CustomUpdateScheduler != null)
                return CustomUpdateScheduler;

            return new List<RuleScheduling>()
            {
                new RuleScheduling(typeof(DummyGameService), SchedulePattern.Default)
            };
        }

        public ErrorPolicy GetErrorPolicy()
        {
            if (CustomErrorPolicy != null)
                return CustomErrorPolicy;

            return new ErrorPolicy()
            {
                IgnoreExceptions = false,
                ReactionOnError = OnErrorBehaviour.PauseJob,
                SkipUnloadIfError = true,
                FallbackMode = null
            };
        }

        public PerformancePolicy GetPerformancePolicy()
        {
            if (CustomPerformancePolicy != null)
                return CustomPerformancePolicy;

            return new PerformancePolicy()
            {
                MaxFrameDuration = 16,
                CheckStallingRules = false
            };
        }
    }
}
