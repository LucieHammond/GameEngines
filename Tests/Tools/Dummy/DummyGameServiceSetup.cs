using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameServiceSetup : IGameServiceSetup
    {
        public const string ID = "TestServices";

        public string Name => CustomName ?? ID;

        public bool CheckAppRequirements()
        {
            return true;
        }

        public string CustomName;
        public IEnumerable<GameRule> CustomServices;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public ExceptionPolicy CustomExceptionPolicy;
        public PerformancePolicy CustomPerformancePolicy;
        public TransitionActivity CustomTransitionActivity;

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
                ReactionDuringLoad = OnExceptionBehaviour.PauseModule,
                ReactionDuringUpdate = OnExceptionBehaviour.SkipFrame,
                ReactionDuringUnload = OnExceptionBehaviour.Continue,
                SkipUnloadIfException = true,
                FallbackModule = null
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

        public TransitionActivity GetTransitionActivity()
        {
            if (CustomTransitionActivity != null)
                return CustomTransitionActivity;

            return null;
        }
    }
}
