using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Scenarios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameEnginesTest.IntegrationTests.PMR
{
    /// <summary>
    /// Integration tests focusing on the interactions between <see cref="GameProcess"/> and <see cref="GameRule"/>
    /// </summary>
    [TestClass]
    public class ProcessToRuleTest
    {
        private PMRScenario m_Scenario;
        private GameProcess m_Process;

        public ProcessToRuleTest()
        {
            m_Scenario = new PMRScenario();
            m_Process = m_Scenario.Process;
        }

        [TestMethod]
        public void ProcessPerformsOperation_RuleMethodsAreCalled()
        {
            int n = 5;

            // Start operation
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(1, m_Scenario.ServiceRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.ServiceRule.UpdateCallCount > 0);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Scenario.ServiceRule.ResetCount();
            m_Scenario.FirstModeRule.ResetCount();

            // Pause operation
            m_Process.Pause();
            m_Scenario.SimulateFrames(n);
            Assert.AreEqual(0, m_Scenario.ServiceRule.UpdateCallCount);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UpdateCallCount);

            // Restart submodule
            m_Process.Restart();
            m_Scenario.SimulateFrames(n);
            Assert.AreEqual(n, m_Scenario.ServiceRule.UpdateCallCount);
            Assert.AreEqual(n, m_Scenario.FirstModeRule.UpdateCallCount);

            // SwitchMode Operation
            m_Process.SwitchToNextGameMode();
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.IsTrue(m_Scenario.ServiceRule.UpdateCallCount > 0);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.SecondModeRule.UpdateCallCount > 0);

            // Stop operation
            m_Process.Stop();
            m_Scenario.SimulateUntil(() => m_Process.Services == null);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.ServiceRule.UnloadCallCount);
        }

        [TestMethod]
        public void ProcessIsAskedToQuit_RuleIsUnloadedInstantly()
        {
            // Load services, game mode and submodule
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            m_Process.CurrentGameMode.LoadSubmodule(m_Scenario.SubmoduleCategory, m_Scenario.SubmoduleSetup);
            GameModule submodule = m_Process.CurrentGameMode.GetSubmodule(m_Scenario.SubmoduleCategory);
            m_Scenario.SimulateUntil(() => submodule.OrchestrationState == OrchestratorState.Operational);

            m_Scenario.ServiceRule.ResetCount();
            m_Scenario.FirstModeRule.ResetCount();
            m_Scenario.SubmoduleRule.ResetCount();

            // Quit operation
            m_Process.OnQuit();
            Assert.AreEqual(1, m_Scenario.SubmoduleRule.OnQuitCallCount);
            Assert.AreEqual(1, m_Scenario.SubmoduleRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.OnQuitCallCount);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.ServiceRule.OnQuitCallCount);
            Assert.AreEqual(1, m_Scenario.ServiceRule.UnloadCallCount);
        }

        [TestMethod]
        public void RuleThrowsException_ProcessLaunchesNewOperation()
        {
            // If an exception is thrown during load, process is paused
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services.OrchestrationState == OrchestratorState.Operational);

            m_Scenario.FirstModeRule.OnInitialize = () => throw new Exception();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.InitializeCallCount > 0);
            m_Scenario.ServiceRule.ResetCount();
            m_Scenario.SimulateFrames(5);
            Assert.AreEqual(0, m_Scenario.ServiceRule.UpdateCallCount);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UpdateCallCount);
            Assert.AreEqual(GameModuleState.InitializeRules, m_Process.CurrentGameMode.State);

            // If an exception is thrown during update, process launches the stop operation
            m_Scenario.FirstModeRule.CallMarkInitialized();
            m_Process.Restart();
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);

            m_Scenario.FirstModeRule.OnUpdate = () => throw new Exception();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            m_Scenario.SimulateUntil(() => m_Process.Services == null);
        }

        private ExceptionPolicy GetTestExceptionPolicy()
        {
            // Exception behaviours corresponds to process operations
            return new ExceptionPolicy()
            {
                ReactionDuringLoad = OnExceptionBehaviour.PauseAll,
                ReactionDuringUpdate = OnExceptionBehaviour.StopAll,
                ReactionDuringUnload = OnExceptionBehaviour.StopAll,
                SkipUnloadIfException = true,
                FallbackModule = m_Scenario.SecondModeSetup
            };
        }
    }
}
