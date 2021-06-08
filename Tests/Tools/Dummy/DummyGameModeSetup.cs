using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameModeSetup : IGameModeSetup
    {
        public const string ID = "TestMode";

        public string Name => CustomName ?? ID;

        public string RequiredServiceSetup => DummyGameServiceSetup.ID;

        public string CustomName;
        public IEnumerable<GameRule> CustomRules;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public ExceptionPolicy CustomExceptionPolicy;
        public PerformancePolicy CustomPerformancePolicy;
        public TransitionActivity CustomTransitionActivity;


        public void SetRules(ref RulesDictionary rules)
        {
            if (CustomRules != null)
            {
                foreach (GameRule rule in CustomRules)
                    rules.AddRule(rule);
            }
            else
            {
                rules.AddRulePack(new DummyGameRulePack());
            }
        }

        public List<Type> GetInitUnloadOrder()
        {
            if (CustomInitUnloadOrder != null)
                return CustomInitUnloadOrder;

            return new List<Type>()
            {
                typeof(DummyGameRule),
                typeof(DummyGameRuleBis),
                typeof(DummyGameRuleTer)
            };
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            if (CustomUpdateScheduler != null)
                return CustomUpdateScheduler;

            return new List<RuleScheduling>()
            {
                new RuleScheduling(typeof(DummyGameRuleBis), 2, 0),
                new RuleScheduling(typeof(DummyGameRule), SchedulePattern.Default),
                new RuleScheduling(typeof(DummyGameRuleTer), 2, 1)
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
