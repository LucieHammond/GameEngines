using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Utils;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Scenarios
{
    public class PMRScenario
    {
        private const int MAX_TESTING_FRAMES = 100;

        public GameProcess Process { get; private set; }

        public StubGameServiceSetup ServiceSetup { get; private set; }
        public StubGameModeSetup FirstModeSetup { get; private set; }
        public StubGameModeSetup SecondModeSetup { get; private set; }
        public StubGameSubmoduleSetup SubmoduleSetup { get; private set; }

        public SpyTransition ServiceTransition { get; private set; }
        public SpyTransition FirstModeTransition { get; private set; }
        public SpyTransition SecondModeTransition { get; private set; }
        public SpyTransition SubmoduleTransition { get; private set; }

        public SpyGameRule ServiceRule { get; private set; }
        public SpyGameRule FirstModeRule { get; private set; }
        public SpyGameRule SecondModeRule { get; private set; }
        public SpyGameRule SubmoduleRule { get; private set; }

        public string SubmoduleCategory => "GameSubmodule";

        private readonly FakeTime m_Time;

        public PMRScenario()
        {
            m_Time = new FakeTime();
            CreateServiceSetup();
            CreateFirstModeSetup();
            CreateSecondModeSetup();
            CreateSubmoduleSetup();

            StubGameProcessSetup setup = new StubGameProcessSetup();
            setup.CustomServiceSetup = ServiceSetup;
            setup.CustomGameModes = new List<IGameModeSetup>() { FirstModeSetup, SecondModeSetup };

            Process = new GameProcess(setup, m_Time);
        }

        public void SimulateUntil(Func<bool> condition)
        {
            if (!Process.SimulateExecutionUntil(m_Time, condition, MAX_TESTING_FRAMES))
                throw new TimeoutException($"PMR Scenario took more than {MAX_TESTING_FRAMES} to execute the requested operation");
        }

        public void SimulateFrames(int nbFrames)
        {
            Process.SimulateExecutionUntil(m_Time, () => false, nbFrames);
        }

        public void ResetFirstGameMode()
        {
            CreateFirstModeSetup();
        }

        private void CreateServiceSetup()
        {
            ServiceRule = new SpyGameRule();
            ServiceRule.SetAutomaticCompletion();
            ServiceTransition = new SpyTransition();
            ServiceTransition.SetAutomaticCompletion();

            ServiceSetup = new StubGameServiceSetup();
            ServiceSetup.CustomRules = new List<GameRule>() { ServiceRule };
            ServiceSetup.CustomInitUnloadOrder = new List<Type>() { typeof(SpyGameRule) };
            ServiceSetup.CustomUpdateScheduler = new List<RuleScheduling>() { new RuleScheduling(typeof(SpyGameRule), 1, 0) };
            ServiceSetup.CustomTransition = ServiceTransition;
        }

        private void CreateFirstModeSetup()
        {
            FirstModeRule = new SpyGameRule();
            FirstModeRule.SetAutomaticCompletion();
            FirstModeTransition = new SpyTransition();
            FirstModeTransition.SetAutomaticCompletion();

            FirstModeSetup = new StubGameModeSetup();
            FirstModeSetup.CustomRules = new List<GameRule>() { FirstModeRule };
            FirstModeSetup.CustomInitUnloadOrder = new List<Type>() { typeof(SpyGameRule) };
            FirstModeSetup.CustomUpdateScheduler = new List<RuleScheduling>() { new RuleScheduling(typeof(SpyGameRule), 1, 0) };
            FirstModeSetup.CustomTransition = FirstModeTransition;
        }

        private void CreateSecondModeSetup()
        {
            SecondModeRule = new SpyGameRule();
            SecondModeRule.SetAutomaticCompletion();
            SecondModeTransition = new SpyTransition();
            SecondModeTransition.SetAutomaticCompletion();

            SecondModeSetup = new StubGameModeSetup();
            SecondModeSetup.CustomRules = new List<GameRule>() { SecondModeRule };
            SecondModeSetup.CustomInitUnloadOrder = new List<Type>() { typeof(SpyGameRule) };
            SecondModeSetup.CustomUpdateScheduler = new List<RuleScheduling>() { new RuleScheduling(typeof(SpyGameRule), 1, 0) };
            SecondModeSetup.CustomTransition = SecondModeTransition;
        }

        private void CreateSubmoduleSetup()
        {
            SubmoduleRule = new SpyGameRule();
            SubmoduleRule.SetAutomaticCompletion();
            SubmoduleTransition = new SpyTransition();
            SubmoduleTransition.SetAutomaticCompletion();

            SubmoduleSetup = new StubGameSubmoduleSetup();
            SubmoduleSetup.CustomRules = new List<GameRule>() { SubmoduleRule };
            SubmoduleSetup.CustomInitUnloadOrder = new List<Type>() { typeof(SpyGameRule) };
            SubmoduleSetup.CustomUpdateScheduler = new List<RuleScheduling>() { new RuleScheduling(typeof(SpyGameRule), 1, 0) };
            SubmoduleSetup.CustomTransition = SubmoduleTransition;
        }
    }
}
