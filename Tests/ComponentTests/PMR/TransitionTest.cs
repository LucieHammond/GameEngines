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
            int n = 3;

            // Initialize transition
            Assert.AreEqual(0, transition.InitializeCallCount);
            transition.BaseInitialize(new FakeTime());
            Assert.AreEqual(1, transition.InitializeCallCount);
            Assert.AreEqual(TransitionState.Inactive, transition.State);

            // Enter transition
            Assert.AreEqual(0, transition.EnterCallCount);
            transition.BaseEnter();
            Assert.AreEqual(1, transition.EnterCallCount);
            Assert.AreEqual(TransitionState.Activating, transition.State);

            // Update n time
            Assert.AreEqual(0, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(n, transition.UpdateCallCount);

            // Mark activation is completed
            transition.CallMarkActivated();
            Assert.AreEqual(TransitionState.Active, transition.State);

            // Update n time
            Assert.AreEqual(n, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(2 * n, transition.UpdateCallCount);

            // Exit transition
            Assert.AreEqual(0, transition.ExitCallCount);
            transition.BaseExit();
            Assert.AreEqual(1, transition.ExitCallCount);
            Assert.AreEqual(TransitionState.Deactivating, transition.State);

            // Update n time
            Assert.AreEqual(2 * n, transition.UpdateCallCount);
            for (int i = 0; i < n; i++)
                transition.BaseUpdate();
            Assert.AreEqual(3 * n, transition.UpdateCallCount);

            // Mark deactivation is completed
            transition.CallMarkDeactivated();
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
            SpyTransition transition = new SpyTransition();
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
