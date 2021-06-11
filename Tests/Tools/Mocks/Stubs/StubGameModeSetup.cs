using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubGameModeSetup : IGameModeSetup
    {
        public string Name => CustomName ?? "TestMode";

        public Type RequiredServiceSetup => CustomRequiredService ?? typeof(StubGameServiceSetup);

        public string CustomName;
        public Type CustomRequiredService;
        public IEnumerable<GameRule> CustomRules;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public List<RuleScheduling> CustomFixedUpdateScheduler;
        public List<RuleScheduling> CustomLateUpdateScheduler;
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
                rules.AddRulePack(new StubGameRulePack());
            }
        }

        public List<Type> GetInitUnloadOrder()
        {
            if (CustomInitUnloadOrder != null)
                return CustomInitUnloadOrder;

            return new List<Type>()
            {
                typeof(StubGameRule),
                typeof(StubGameRuleBis),
                typeof(StubGameRuleTer)
            };
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            if (CustomUpdateScheduler != null)
                return CustomUpdateScheduler;

            return new List<RuleScheduling>()
            {
                new RuleScheduling(typeof(StubGameRuleBis), 2, 0),
                new RuleScheduling(typeof(StubGameRule), SchedulePattern.Default),
                new RuleScheduling(typeof(StubGameRuleTer), 2, 1)
            };
        }

        public List<RuleScheduling> GetFixedUpdateScheduler()
        {
            if (CustomFixedUpdateScheduler != null)
                return CustomFixedUpdateScheduler;

            return new List<RuleScheduling>();
        }

        public List<RuleScheduling> GetLateUpdateScheduler()
        {
            if (CustomLateUpdateScheduler != null)
                return CustomLateUpdateScheduler;

            return new List<RuleScheduling>();
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
