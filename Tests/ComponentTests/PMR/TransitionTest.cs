using GameEngine.Core.System;
using GameEngine.PMR.Process.Transitions;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Spies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class Transition
    /// <see cref="Transition"/>
    /// </summary>
    [TestClass]
    public class TransitionTest
    {
        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            SpyTransition transition = new SpyTransition();
            transition.Configure(new FakeTime(), new Configuration());
            int n = 3;

            // Prepare transition
            Assert.AreEqual(0, transition.PrepareCallCount);
            transition.BasePrepare();
            Assert.AreEqual(1, transition.PrepareCallCount);
            Assert.AreEqual(TransitionState.Inactive, transition.State);

            // Mark as ready
            Assert.IsFalse(transition.IsReady);
            transition.CallMarkReady();
            Assert.IsTrue(transition.IsReady);

            // Enter transition (and update)
            Assert.AreEqual(0, transition.EnterCallCount);
            transition.BaseEnter();
            Assert.AreEqual(1, transition.EnterCallCount);
            Assert.AreEqual(TransitionState.Entering, transition.State);

            // Execute entry (update n times)
            for (int i = 0; i < n; i++) transition.BaseUpdate();
            Assert.AreEqual(n, transition.UpdateCallCount);

            // Mark as entered
            transition.CallMarkEntered();
            Assert.AreEqual(TransitionState.Running, transition.State);

            // Run transition (update n times)
            for (int i = 0; i < n; i++) transition.BaseUpdate();
            Assert.AreEqual(2 * n, transition.UpdateCallCount);

            // Mark as completed
            Assert.IsFalse(transition.IsComplete);
            transition.CallMarkCompleted();
            Assert.IsTrue(transition.IsComplete);

            // Exit transition (and update)
            Assert.AreEqual(0, transition.ExitCallCount);
            transition.BaseExit();
            Assert.AreEqual(1, transition.ExitCallCount);
            Assert.AreEqual(TransitionState.Exiting, transition.State);

            // Execute exit (update n times)
            for (int i = 0; i < n; i++) transition.BaseUpdate();
            Assert.AreEqual(3 * n, transition.UpdateCallCount);

            // Mark as exited
            transition.CallMarkExited();
            Assert.AreEqual(TransitionState.Inactive, transition.State);

            // Cleanup transition
            Assert.AreEqual(0, transition.CleanupCallCount);
            transition.BaseCleanup();
            Assert.AreEqual(1, transition.CleanupCallCount);
            Assert.AreEqual(TransitionState.Inactive, transition.State);
        }

        [TestMethod]
        public void AccessLoadingReports()
        {
            // Configure transition
            SpyTransition transition = new SpyTransition();
            Configuration moduleConfig = new Configuration() { { "key", "value" } };
            transition.Configure(new FakeTime(), moduleConfig);
            Assert.AreEqual(moduleConfig, transition.ModuleConfiguration);

            // At initialization, loading reports have default values
            transition.BasePrepare();
            Assert.AreEqual(0, transition.LoadingProgress);
            Assert.AreEqual(string.Empty, transition.LoadingAction);

            // Access loading progress when reported
            float loadingProgress = 0.57f;
            transition.ReportLoadingProgress(loadingProgress);
            Assert.AreEqual(loadingProgress, transition.LoadingProgress);

            // Access loading action when reported
            string loadingAction = "Test loading action";
            transition.ReportLoadingAction(loadingAction);
            Assert.AreEqual(loadingAction, transition.LoadingAction);

            // The progress values reported are ajusted if out of range
            transition.ReportLoadingProgress(50.0f);
            Assert.AreEqual(1, transition.LoadingProgress);
        }
    }
}
