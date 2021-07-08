using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Specialization;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class GameModule
    /// <see cref="GameModule"/>
    /// </summary>
    [TestClass]
    public class GameModuleTest
    {
        private FakeTime m_Time;

        public GameModuleTest()
        {
            m_Time = new FakeTime();
        }

        [TestMethod]
        public void HasCorrectConfiguration()
        {
            GameModule module = CreateValidGameModule(out IGameModuleSetup setup, out Configuration configuration);

            // Module Id and Name correspond to the setup
            Assert.AreEqual(setup.GetType(), module.Id);
            Assert.AreEqual(setup.Name, module.Name);

            // Module Configuration is the same as the provided one
            Assert.AreEqual(configuration, module.Configuration);

            // Module State is set to Start
            Assert.AreEqual(GameModuleState.Start, module.State);

            // Module contains a correct reference to an orchestrator
            Assert.IsNotNull(module.Orchestrator);
            Assert.AreEqual(module.Orchestrator.State, module.OrchestrationState);
        }

        [TestMethod]
        public void CompleteLoadingSequence()
        {
            GameModule module = CreateValidGameModule(out IGameModuleSetup _, out Configuration _);

            // Trigger the load sequence from the Start state
            bool loadingCompleted = false;
            module.OnFinishLoading += () => loadingCompleted = true;
            module.InnerLoad();
            Assert.AreEqual(GameModuleState.Start, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));

            // Perform Configure state
            Assert.AreEqual(GameModuleState.Configure, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            Assert.IsNotNull(module.Rules);
            Assert.IsNotNull(module.InitUnloadOrder);
            Assert.IsNotNull(module.UpdateScheduler);
            Assert.IsNotNull(module.ExceptionPolicy);
            Assert.IsNotNull(module.PerformancePolicy);

            // Perform InjectDependencies state
            Assert.AreEqual(GameModuleState.InjectDependencies, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            Assert.IsNotNull(module.DependencyProvider);
            Assert.IsTrue(module.DependencyProvider.TryGet(typeof(IStubGameRuleBis), out object rubeBis));
            Assert.AreEqual(rubeBis, ((StubGameRuleTer)module.Rules[typeof(StubGameRuleTer)]).StubRuleBisReference);
            Assert.IsNull(((StubGameRuleTer)module.Rules[typeof(StubGameRuleTer)]).StubServiceReference);

            // Perform Pre-Initialization state
            Assert.AreEqual(GameModuleState.PreInitialize, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (SpecializedTask task in module.SpecializedTasks)
                Assert.IsTrue(task.State == SpecializedTaskState.InitCompleted);

            // Perform InitializeRules state
            Assert.AreEqual(GameModuleState.InitializeRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (GameRule rule in module.Rules.Values)
                Assert.AreEqual(GameRuleState.Initialized, rule.State);

            // End up in UpdateRules state
            Assert.IsTrue(loadingCompleted);
            Assert.AreEqual(GameModuleState.UpdateRules, module.State);
        }

        [TestMethod]
        public void CompleteUnloadingSequence()
        {
            GameModule module = CreateValidGameModule(out IGameModuleSetup _, out Configuration _);
            module.InnerLoad();
            module.SimulateExecutionUntil(m_Time, () => module.State == GameModuleState.UpdateRules);

            // Trigger the unload sequence from the UpdateRules state
            bool unloadingCompleted = false;
            module.OnFinishUnloading += () => unloadingCompleted = true;
            module.InnerUnload();
            Assert.AreEqual(GameModuleState.UpdateRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));

            // Perform UnloadRules state
            Assert.AreEqual(GameModuleState.UnloadRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (GameRule rule in module.Rules.Values)
                Assert.AreEqual(GameRuleState.Unloaded, rule.State);

            // Perform Post-Unload state
            Assert.AreEqual(GameModuleState.PostUnload, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (SpecializedTask task in module.SpecializedTasks)
                Assert.IsTrue(task.State == SpecializedTaskState.UnloadCompleted);

            // End up in End state
            Assert.IsTrue(unloadingCompleted);
            Assert.AreEqual(GameModuleState.End, module.State);

            // Can trigger unload from InitializeRules state
            module = CreateValidGameModule(out IGameModuleSetup _, out Configuration _);
            module.InnerLoad();
            module.SimulateExecutionUntil(m_Time, () => module.State == GameModuleState.InitializeRules);
            module.InnerUnload();
            Assert.IsTrue(module.SimulateExecutionUntil(m_Time, () => module.State == GameModuleState.End));
        }

        [TestMethod]
        public void CompleteReloadingSequence()
        {
            GameModule module = CreateValidGameModule(out IGameModuleSetup _, out Configuration _);
            module.InnerLoad();
            module.SimulateExecutionUntil(m_Time, () => module.State == GameModuleState.UpdateRules);

            // Trigger the reload sequence from the UpdateRules state
            bool reloadingCompleted = false;
            module.OnFinishLoading += () => reloadingCompleted = true;
            module.InnerReload();
            Assert.AreEqual(GameModuleState.UpdateRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));

            // Perform UnloadRules state
            Assert.AreEqual(GameModuleState.UnloadRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (GameRule rule in module.Rules.Values)
                Assert.AreEqual(GameRuleState.Unloaded, rule.State);

            // Perform Post-Unload state
            Assert.AreEqual(GameModuleState.PostUnload, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));

            // Perform Pre-Initialization state
            Assert.AreEqual(GameModuleState.PreInitialize, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));

            // Perform InitializeRules state
            Assert.AreEqual(GameModuleState.InitializeRules, module.State);
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            foreach (GameRule rule in module.Rules.Values)
                Assert.AreEqual(GameRuleState.Initialized, rule.State);

            // End up in UpdateRules state
            Assert.IsTrue(reloadingCompleted);
            Assert.AreEqual(GameModuleState.UpdateRules, module.State);
        }

        [TestMethod]
        public void CanBePausedAndStopped()
        {
            GameModule module = CreateValidGameModule(out IGameModuleSetup _, out Configuration _);
            module.InnerUnload();

            // The module is paused -> no progression is made and the simulation exceeds timeout
            module.Pause();
            Assert.IsFalse(module.SimulateUpToNextState(m_Time));

            // The module is restarted -> operation resumes without error
            module.Restart();
            Assert.IsTrue(module.SimulateUpToNextState(m_Time));
            Assert.AreEqual(GameModuleState.End, module.State);

            // The module is abruptly stopped -> no exception is thrown
            module.InnerQuit();
        }

        [TestMethod]
        public void ManageSetupExceptions()
        {
            StubGameModeSetup invalidSetup;
            GameModule invalidModule;
            bool simulateUntilState(GameModuleState state) => invalidModule.SimulateExecutionUntil(m_Time, () => invalidModule.State == state);

            // If setup rules order contains duplicated rules -> Configure state logs exception and responds by pausing the module
            invalidSetup = new StubGameModeSetup();
            invalidSetup.CustomInitUnloadOrder = new List<Type>() { typeof(StubGameRule), typeof(StubGameRuleBis), typeof(StubGameRule) };
            invalidModule = CreateCustomGameModule(invalidSetup);
            invalidModule.InnerLoad();
            AssertUtils.LogException<Exception>(() => Assert.IsFalse(simulateUntilState(GameModuleState.InjectDependencies)));
            Assert.AreEqual(GameModuleState.Configure, invalidModule.State);

            // If setup exception policy is invalid -> Configure state logs exception and responds by pausing the module
            invalidSetup = new StubGameModeSetup();
            invalidSetup.CustomExceptionPolicy = new ExceptionPolicy()
            {
                ReactionDuringUpdate = OnExceptionBehaviour.SwitchToFallback,
                FallbackModule = null
            };
            invalidModule = CreateCustomGameModule(invalidSetup);
            invalidModule.InnerLoad();
            AssertUtils.LogException<Exception>(() => Assert.IsFalse(simulateUntilState(GameModuleState.InjectDependencies)));
            Assert.AreEqual(GameModuleState.Configure, invalidModule.State);

            // If setup performance policy is invalid -> Configure state logs exception and responds by pausing the module
            invalidSetup = new StubGameModeSetup();
            invalidSetup.CustomPerformancePolicy = new PerformancePolicy()
            {
                MaxFrameDuration = 20,
                CheckStallingRules = true,
                InitStallingTimeout = 100,
                UpdateStallingTimeout = 100,
                UnloadStallingTimeout = 0
            };
            invalidModule = CreateCustomGameModule(invalidSetup);
            invalidModule.InnerLoad();
            AssertUtils.LogException<Exception>(() => Assert.IsFalse(simulateUntilState(GameModuleState.InjectDependencies)));
            Assert.AreEqual(GameModuleState.Configure, invalidModule.State);

            // If one of the rule cannot find a required dependency -> InjectDependencies state logs exception and responds by pausing the module
            invalidSetup = new StubGameModeSetup();
            invalidSetup.CustomRules = new List<GameRule>() { new StubGameRuleTer() };
            invalidModule = CreateCustomGameModule(invalidSetup);
            invalidModule.InnerLoad();
            AssertUtils.LogException<Exception>(() => Assert.IsFalse(simulateUntilState(GameModuleState.InitializeRules)));
            Assert.AreEqual(GameModuleState.InjectDependencies, invalidModule.State);

            // Even when having errors during setup, the module can be unloaded correctly
            invalidModule.Restart();
            invalidModule.InnerUnload();
            Assert.IsTrue(simulateUntilState(GameModuleState.End));
        }

        private GameModule CreateValidGameModule(out IGameModuleSetup setup, out Configuration configuration)
        {
            setup = new StubGameModeSetup();
            configuration = new Configuration() { { "parameter", "value" } };

            return CreateCustomGameModule(setup, configuration);
        }

        private GameModule CreateCustomGameModule(IGameModuleSetup setup, Configuration configuration = null)
        {
            GameProcess process = new GameProcess(new StubGameProcessSetup(), m_Time);
            Orchestrator orchestrator = new Orchestrator("Test", process, null);
            orchestrator.CurrentModule = new GameModule(setup, configuration, orchestrator);

            return orchestrator.CurrentModule;
        }
    }
}
