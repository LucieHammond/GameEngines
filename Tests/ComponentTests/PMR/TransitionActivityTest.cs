using GameEngine.PMR.Modules.Transitions;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Spies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class TransitionActivity
    /// <see cref="TransitionActivity"/>
    /// </summary>
    [TestClass]
    public class TransitionActivityTest
    {
        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            SpyTransitionActivity transition = new SpyTransitionActivity();
            int n = 3;

            // Initialize transition
            Assert.AreEqual(0, transition.InitializeCallCount);
            transition.BaseInitialize(new FakeTime());
            Assert.AreEqual(1, transition.InitializeCallCount);
            Assert.AreEqual(TransitionState.Inactive, transition.State);

            // Start transition
            Assert.AreEqual(0, transition.StartCallCount);
            transition.BaseStart();
            Assert.AreEqual(1, transition.StartCallCount);
            Assert.AreEqual(TransitionState.Starting, transition.State);

            // Update n time
            Assert.AreEqual(0, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(n, transition.UpdateCallCount);

            // Mark start completed
            transition.CallMarkStartCompleted();
            Assert.AreEqual(TransitionState.Active, transition.State);

            // Update n time
            Assert.AreEqual(n, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(2 * n, transition.UpdateCallCount);

            // Stop transition
            Assert.AreEqual(0, transition.StopCallCount);
            transition.BaseStop();
            Assert.AreEqual(1, transition.StopCallCount);
            Assert.AreEqual(TransitionState.Stopping, transition.State);

            // Update n time
            Assert.AreEqual(2 * n, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(3 * n, transition.UpdateCallCount);

            // Mark stop completed
            transition.CallMarkStopCompleted();
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
            // At initialization, loading reports have default values
            SpyTransitionActivity transition = new SpyTransitionActivity();
            transition.BaseInitialize(new FakeTime());
            Assert.AreEqual(0, transition.LoadingProgress);
            Assert.AreEqual(string.Empty, transition.LoadingAction);

            // If UseDefaultReport = false, loading progress is not affected by ReportDefaultProgress internal method
            float defaultProgress = 0.23f;
            transition.UseDefaultReport = false;
            transition.SetDefaultProgress(defaultProgress);
            Assert.AreEqual(0, transition.LoadingProgress);

            // If UseDefaultReport = true, loading progress takes its values from ReportDefaultProgress internal method
            transition.UseDefaultReport = true;
            transition.SetDefaultProgress(defaultProgress);
            Assert.AreEqual(defaultProgress, transition.LoadingProgress);

            // Loading progress and loading action can be modified with public setters
            float customProgress = 0.57f;
            string customAction = "Test loading action";
            transition.ReportLoadingProgress(customProgress);
            Assert.AreEqual(customProgress, transition.LoadingProgress);
            transition.ReportLoadingAction(customAction);
            Assert.AreEqual(customAction, transition.LoadingAction);

            // The values reported with public setters are ajusted if out of range
            transition.ReportLoadingProgress(50.0f);
            Assert.AreEqual(1, transition.LoadingProgress);
        }
    }
}
