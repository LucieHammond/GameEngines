using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameServiceSetup : IGameServiceSetup
    {
        public string Name => CustomName ?? "Test";

        public string CustomName;
        public IEnumerable<GameRule> CustomServices;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public ExceptionPolicy CustomExceptionPolicy;
        public PerformancePolicy CustomPerformancePolicy;

        public void SetRules(ref RulesDictionary rules)
        {
            if (CustomServices != null)
            {
                foreach (GameRule service in CustomServices)
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

        public ExceptionPolicy GetExceptionPolicy()
        {
            if (CustomExceptionPolicy != null)
                return CustomExceptionPolicy;

            return new ExceptionPolicy()
            {
                ReactionDuringLoad = OnExceptionBehaviour.PauseJob,
                ReactionDuringUpdate = OnExceptionBehaviour.SkipFrame,
                ReactionDuringUnload = OnExceptionBehaviour.Continue,
                SkipUnloadIfException = true,
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
