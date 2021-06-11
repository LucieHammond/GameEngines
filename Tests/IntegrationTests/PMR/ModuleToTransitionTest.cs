using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Scenarios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.IntegrationTests.PMR
{
    /// <summary>
    /// Integration tests focusing on the interactions between <see cref="GameModule"/> and <see cref="TransitionActivity"/>
    /// </summary>
    [TestClass]
    public class ModuleToTransitionTest
    {
        private PMRScenario m_Scenario;
        private GameProcess m_Process;

        public ModuleToTransitionTest()
        {
            m_Scenario = new PMRScenario();
            m_Process = m_Scenario.Process;
        }

        [TestMethod]
        public void ModulePerformsOperation_TransitionMethodsAreCalled()
        {
            m_Process.Start();

            // First load operation
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(1, m_Scenario.ServiceTransition.InitializeCallCount);
            AssertTransitionCompleted(m_Scenario.ServiceTransition);
            Assert.AreEqual(1, m_Scenario.FirstModeTransition.InitializeCallCount);
            AssertTransitionCompleted(m_Scenario.FirstModeTransition);
            m_Scenario.FirstModeTransition.ResetCount();

            // Reload operation
            m_Process.CurrentGameMode.Reload();
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            AssertTransitionCompleted(m_Scenario.FirstModeTransition);
            m_Scenario.FirstModeTransition.ResetCount();

            // Switch operation
            m_Process.CurrentGameMode.SwitchToModule(m_Scenario.SecondModeSetup);
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(1, m_Scenario.FirstModeTransition.CleanupCallCount);
            Assert.AreEqual(1, m_Scenario.SecondModeTransition.InitializeCallCount);
            AssertTransitionCompleted(m_Scenario.SecondModeTransition);
            m_Scenario.SecondModeTransition.ResetCount();

            // LoadSubmodule operation
            m_Process.CurrentGameMode.LoadSubmodule(m_Scenario.SubmoduleCategory, m_Scenario.SubmoduleSetup);
            GameModule submodule = m_Process.CurrentGameMode.GetSubmodule(m_Scenario.SubmoduleCategory);
            m_Scenario.SimulateUntil(() => submodule.OrchestrationState == OrchestratorState.Operational);
            Assert.AreEqual(1, m_Scenario.SubmoduleTransition.InitializeCallCount);
            AssertTransitionCompleted(m_Scenario.SubmoduleTransition);
            m_Scenario.SubmoduleTransition.ResetCount();

            // UnloadSubmodule operation
            m_Process.CurrentGameMode.UnloadSubmodule(m_Scenario.SubmoduleCategory);
            m_Scenario.SimulateUntil(() => submodule.OrchestrationState == OrchestratorState.Wait);
            AssertTransitionCompleted(m_Scenario.SubmoduleTransition);
            Assert.AreEqual(1, m_Scenario.SubmoduleTransition.CleanupCallCount);
            m_Scenario.SubmoduleTransition.ResetCount();

            // Unload operation
            m_Process.CurrentGameMode.Unload();
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(1, m_Scenario.SecondModeTransition.StartCallCount);
            Assert.IsTrue(m_Scenario.SecondModeTransition.UpdateCallCount > 0);
        }

        [TestMethod]
        public void ModuleIsLoading_TransitionProgressIsUpdated()
        {
            // Setup 2 rules in first game mode: a standard StubGameRule and the controllable FirstModeRule
            m_Scenario.FirstModeSetup.CustomRules = new List<GameRule>() { new StubGameRule(), m_Scenario.FirstModeRule };
            m_Scenario.FirstModeSetup.CustomInitUnloadOrder = new List<Type>() { typeof(StubGameRule), typeof(SpyGameRule) };
            m_Scenario.FirstModeRule.OnInitialize = null;

            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services.OrchestrationState == OrchestratorState.Operational);

            // When first game mode begins loading: progress = 0
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.Setup);
            Assert.AreEqual(0, m_Scenario.FirstModeTransition.LoadingProgress);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InjectDependencies);
            Assert.AreEqual(0, m_Scenario.FirstModeTransition.LoadingProgress);

            // When StubGameRule is initialized: progress = 0.5
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InitializeRules
                && m_Scenario.FirstModeRule.InitializeCallCount > 0);
            Assert.AreEqual(0.5, m_Scenario.FirstModeTransition.LoadingProgress);

            // When FirstModeRule is initialized: progress = 1
            m_Scenario.FirstModeRule.CallMarkInitialized();
            m_Scenario.SimulateFrames(1);
            Assert.AreEqual(1, m_Scenario.FirstModeTransition.LoadingProgress);

            // Set custom report
            m_Process.CurrentGameMode.GetTransition().ReportLoadingProgress(0.7f);
            Assert.AreEqual(0.7f, m_Scenario.FirstModeTransition.LoadingProgress);
        }

        private void AssertTransitionCompleted(SpyTransitionActivity transition)
        {
            Assert.AreEqual(1, transition.StartCallCount);
            Assert.IsTrue(transition.UpdateCallCount > 0);
            Assert.AreEqual(1, transition.StopCallCount);
        }
    }
}
