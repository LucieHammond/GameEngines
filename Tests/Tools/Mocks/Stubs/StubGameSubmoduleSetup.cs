using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process.Transitions;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubGameSubmoduleSetup : IGameModuleSetup
    {
        public string Name => CustomName ?? "TestSubmodule";

        public Type RequiredServiceSetup => CustomRequiredService ?? typeof(StubGameServiceSetup);

        public Type RequiredParentSetup => CustomRequiredParent ?? typeof(StubGameModeSetup);

        public string CustomName;
        public Type CustomRequiredService;
        public Type CustomRequiredParent;
        public IEnumerable<GameRule> CustomRules;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public List<RuleScheduling> CustomFixedUpdateScheduler;
        public List<RuleScheduling> CustomLateUpdateScheduler;
        public ExceptionPolicy CustomExceptionPolicy;
        public PerformancePolicy CustomPerformancePolicy;
        public Transition CustomTransition;

        public void SetRules(ref RulesDictionary rules)
        {
            if (CustomRules != null)
            {
                foreach (GameRule rule in CustomRules)
                    rules.AddRule(rule);
            }
        }

        public List<Type> GetInitUnloadOrder()
        {
            if (CustomInitUnloadOrder != null)
                return CustomInitUnloadOrder;

            return new List<Type>();
        }

        public List<RuleScheduling> GetUpdateScheduler()
        {
            if (CustomUpdateScheduler != null)
                return CustomUpdateScheduler;

            return new List<RuleScheduling>();
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

        public Transition GetTransition()
        {
            if (CustomTransition != null)
                return CustomTransition;

            return null;
        }
    }
}
