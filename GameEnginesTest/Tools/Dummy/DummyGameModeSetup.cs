﻿using GameEngine.PJR.Jobs.Policies;
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

        public List<GameRule> CustomRules;
        public List<Type> CustomInitUnloadOrder;
        public List<RuleScheduling> CustomUpdateScheduler;
        public ErrorPolicy CustomErrorPolicy;
        public PerformancePolicy CustomPerformancePolicy;

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
