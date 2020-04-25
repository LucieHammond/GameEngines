using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Dependencies;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.ComponentTests.PJR
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
            // Use Dummy implementation of GameRule since GameRule is abstract
            DummyGameRule rule = new DummyGameRule();
            Assert.AreEqual("DummyGameRule", rule.Name);
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
            DummyGameRule rule1 = new DummyGameRule();
            rule1.OnInitialize += () => rule1.CallMarkError();
            
            Assert.IsFalse(rule1.ErrorDetected);
            rule1.BaseInitialize();
            Assert.IsTrue(rule1.ErrorDetected);
            Assert.AreEqual(GameRuleState.Unloaded, rule1.State);

            // Report error during update
            DummyGameRule rule2 = new DummyGameRule();
            rule2.OnInitialize += () => rule2.CallMarkInitialized();
            rule2.OnUpdate += () => rule2.CallMarkError();
            rule2.BaseInitialize();
            
            Assert.IsFalse(rule2.ErrorDetected);
            rule2.BaseUpdate();
            Assert.IsTrue(rule2.ErrorDetected);
            Assert.AreEqual(GameRuleState.Initialized, rule2.State);

            // Report error during unload
            DummyGameRule rule3 = new DummyGameRule();
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
            DummyGameRule rule = new DummyGameRule();
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
        public void AccessProcessAfterInjection()
        {
            // Trying to access process without dependency injection -> throws InvalidOperationException
            DummyGameRule rule = new DummyGameRule();
            Assert.ThrowsException<InvalidOperationException>(() => rule.Process);

            // Create a GameProcess and simulate a dependency injection via a ProcessService acting as provider
            GameProcess process = new GameProcess(new DummyGameProcessSetup(), new MockProcessTime());
            DependencyProvider serviceProvider = new DependencyProvider();
            serviceProvider.Add(typeof(IProcessAccessor), new ProcessService(process));
            RulesDictionary rulesToInject = new RulesDictionary();
            rulesToInject.AddRule(rule);
            DependencyUtils.InjectDependencies(rulesToInject, serviceProvider, null, null);

            // Trying to access process after dependency injection -> process is correctly returned
            Assert.AreEqual(process, rule.Process);
            Assert.IsNotNull(rule.Process.Time);
        }
    }
}
