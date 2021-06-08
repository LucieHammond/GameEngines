using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Mocks.Stubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class GameRule
    /// <see cref="GameRule"/>
    /// </summary>
    [TestClass]
    public class GameRuleTest
    {
        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            // Create a rule
            SpyGameRule rule = new SpyGameRule();
            Assert.AreEqual("SpyGameRule", rule.Name);
            Assert.AreEqual(GameRuleState.Unused, rule.State);
            Assert.AreEqual(0, rule.InitializeCallCount);
            Assert.AreEqual(0, rule.UpdateCallCount);
            Assert.AreEqual(0, rule.UnloadCallCount);

            // Start initializing
            rule.BaseInitialize();
            Assert.AreEqual(GameRuleState.Initializing, rule.State);
            Assert.AreEqual(1, rule.InitializeCallCount);
            Assert.AreEqual(0, rule.UpdateCallCount);
            Assert.AreEqual(0, rule.UnloadCallCount);

            // Mark initialize
            rule.CallMarkInitialized();
            Assert.AreEqual(GameRuleState.Initialized, rule.State);

            // Update n times
            int n = 3;
            for (int i = 0; i < n; i++)
                rule.BaseUpdate();
            Assert.AreEqual(GameRuleState.Initialized, rule.State);
            Assert.AreEqual(1, rule.InitializeCallCount);
            Assert.AreEqual(n, rule.UpdateCallCount);
            Assert.AreEqual(0, rule.UnloadCallCount);

            // Start unloading
            rule.BaseUnload();
            Assert.AreEqual(GameRuleState.Unloading, rule.State);
            Assert.AreEqual(1, rule.InitializeCallCount);
            Assert.AreEqual(n, rule.UpdateCallCount);
            Assert.AreEqual(1, rule.UnloadCallCount);

            // Mark unloaded
            rule.CallMarkUnloaded();
            Assert.AreEqual(GameRuleState.Unloaded, rule.State);
        }

        [TestMethod]
        public void ReportErrorsAtAnyTime()
        {
            // Report error during initialization
            SpyGameRule rule1 = new SpyGameRule();
            rule1.OnInitialize += () => rule1.CallMarkError();

            Assert.IsFalse(rule1.ErrorDetected);
            rule1.BaseInitialize();
            Assert.IsTrue(rule1.ErrorDetected);
            Assert.AreEqual(GameRuleState.Unloaded, rule1.State);

            // Report error during update
            SpyGameRule rule2 = new SpyGameRule();
            rule2.OnInitialize += () => rule2.CallMarkInitialized();
            rule2.OnUpdate += () => rule2.CallMarkError();
            rule2.BaseInitialize();

            Assert.IsFalse(rule2.ErrorDetected);
            rule2.BaseUpdate();
            Assert.IsTrue(rule2.ErrorDetected);
            Assert.AreEqual(GameRuleState.Unloaded, rule2.State);

            // Report error during unload
            SpyGameRule rule3 = new SpyGameRule();
            rule3.OnInitialize += () => rule3.CallMarkInitialized();
            rule3.OnUnload += () => rule3.CallMarkError();
            rule3.BaseInitialize();
            rule3.BaseUpdate();

            Assert.IsFalse(rule3.ErrorDetected);
            rule3.BaseUnload();
            Assert.IsTrue(rule3.ErrorDetected);
            Assert.AreEqual(GameRuleState.Unloaded, rule3.State);
        }

        [TestMethod]
        public void ManageAbruptExit()
        {
            // Create dummy GameRule
            SpyGameRule rule = new SpyGameRule();
            rule.OnInitialize += () => rule.CallMarkInitialized();
            rule.BaseInitialize();
            Assert.AreEqual(0, rule.UnloadCallCount);
            Assert.AreEqual(0, rule.OnQuitCallCount);

            // Execute exit operations (by default, OnQuit calls Unload)
            rule.BaseQuit();
            Assert.AreEqual(1, rule.UnloadCallCount);
            Assert.AreEqual(1, rule.OnQuitCallCount);
        }

        [TestMethod]
        public void AccessProcessAndModule()
        {
            // Trying to access current process and module before dependency injection -> return null
            SpyGameRule rule = new SpyGameRule();
            Assert.IsNull(rule.Process);
            Assert.IsNull(rule.CurrentModule);

            // Create a GameProcess and a GameModule in order to simulate a dependency injection 
            GameProcess process = new GameProcess(new StubGameProcessSetup(), new FakeTime());
            Orchestrator orchestrator = new Orchestrator("Test", process, null);
            GameModule module = new GameModule(new StubGameModeSetup(), null, orchestrator);
            rule.InjectProcessDependencies(process, module);

            // Trying to access process and module after dependency injection -> return correct info
            Assert.AreEqual(process, rule.Process);
            Assert.AreEqual(module, rule.CurrentModule);
            Assert.IsNotNull(rule.Process.Time);
        }
    }
}
