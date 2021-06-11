using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Scenarios;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace GameEnginesTest.IntegrationTests.PMR
{
    /// <summary>
    /// Integration tests focusing on the interactions between <see cref="GameModule"/> and <see cref="GameRule"/>
    /// </summary>
    [TestClass]
    public class ModuleToRuleTest
    {
        private PMRScenario m_Scenario;
        private GameProcess m_Process;

        public ModuleToRuleTest()
        {
            m_Scenario = new PMRScenario();
            m_Process = m_Scenario.Process;
        }

        [TestMethod]
        public void ModulePerformsOperation_RulesMethodsAreCalled()
        {
            m_Process.Start();

            // First load operation
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.FirstModeRule.UpdateCallCount > 0);

            // Reload operation
            m_Process.CurrentGameMode.Reload();
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(2, m_Scenario.FirstModeRule.InitializeCallCount);

            // Switch operation
            m_Process.CurrentGameMode.SwitchToModule(m_Scenario.SecondModeSetup);
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            Assert.AreEqual(2, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.SecondModeRule.UpdateCallCount > 0);

            // LoadSubmodule operation
            m_Process.CurrentGameMode.LoadSubmodule(m_Scenario.SubmoduleCategory, m_Scenario.SubmoduleSetup);
            GameModule submodule = m_Process.CurrentGameMode.GetSubmodule(m_Scenario.SubmoduleCategory);
            m_Scenario.SimulateUntil(() => submodule.OrchestrationState == OrchestratorState.Operational);
            Assert.AreEqual(1, m_Scenario.SubmoduleRule.InitializeCallCount);
            Assert.IsTrue(m_Scenario.SubmoduleRule.UpdateCallCount > 0);

            // UnloadSubmodule operation
            m_Process.CurrentGameMode.UnloadSubmodule(m_Scenario.SubmoduleCategory);
            m_Scenario.SimulateUntil(() => submodule.OrchestrationState == OrchestratorState.Wait);
            Assert.AreEqual(1, m_Scenario.SubmoduleRule.UnloadCallCount);

            // Pause operation
            m_Process.CurrentGameMode.Pause();
            m_Scenario.SecondModeRule.ResetCount();
            m_Scenario.SimulateFrames(5);
            Assert.AreEqual(0, m_Scenario.SecondModeRule.UpdateCallCount);

            // Restart operation
            m_Process.CurrentGameMode.Restart();
            m_Scenario.SecondModeRule.ResetCount();
            m_Scenario.SimulateFrames(5);
            Assert.AreEqual(5, m_Scenario.SecondModeRule.UpdateCallCount);

            // Unload operation
            m_Process.CurrentGameMode.Unload();
            m_Scenario.SimulateFrames(1);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.UnloadCallCount);
        }

        [TestMethod]
        public void RuleTransmitsError_ModuleIsUnloaded()
        {
            // The error is reported during the loading of the first mode
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services.OrchestrationState == OrchestratorState.Operational);

            m_Scenario.FirstModeRule.OnInitialize = () => m_Scenario.FirstModeRule.CallMarkError();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.InitializeCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UnloadCallCount);

            // The error is reported during the update of the first mode
            m_Scenario.ResetFirstGameMode();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            m_Scenario.FirstModeRule.ResetCount();

            m_Scenario.FirstModeRule.OnUpdate = () => m_Scenario.FirstModeRule.CallMarkError();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UnloadCallCount);

            // The error is reported during the unloading of the first mode
            m_Scenario.ResetFirstGameMode();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);

            m_Scenario.FirstModeRule.OnUnload = () => m_Scenario.FirstModeRule.CallMarkError();
            m_Process.CurrentGameMode.Unload();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UnloadCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
        }

        [TestMethod]
        public void RuleThrowException_ModuleLaunchesNewOperation()
        {
            // If an exception is thrown during load, module launches the unload operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services.OrchestrationState == OrchestratorState.Operational);

            m_Scenario.FirstModeRule.OnInitialize = () => throw new Exception();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.InitializeCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UnloadCallCount);
            m_Scenario.ResetFirstGameMode();

            // If an exception is thrown during update, module launches the reload operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            m_Scenario.FirstModeRule.ResetCount();

            m_Scenario.FirstModeRule.OnUpdate = () => throw new Exception();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Scenario.FirstModeRule.OnUpdate = null;
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.InitializeCallCount);
            m_Scenario.ResetFirstGameMode();

            // If an exception is thrown during unload, module launches the switchToFallback operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);

            m_Scenario.FirstModeRule.OnUnload = () => throw new Exception();
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Process.CurrentGameMode.Unload();
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.InitializeCallCount);
        }

        [TestMethod]
        public void RuleIsStalling_ModuleLaunchesNewOperation()
        {
            // If rule is stalling during load, module launches the unload operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Scenario.FirstModeSetup.CustomPerformancePolicy = GetTestPerformancePolicy();
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services.OrchestrationState == OrchestratorState.Operational);

            m_Scenario.FirstModeRule.OnInitialize = null;
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.InitializeCallCount > 0);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            Assert.AreEqual(0, m_Scenario.FirstModeRule.UnloadCallCount);
            m_Scenario.ResetFirstGameMode();

            // If rule is stalling during update, module launches the reload operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Scenario.FirstModeSetup.CustomPerformancePolicy = GetTestPerformancePolicy();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);
            m_Scenario.FirstModeRule.ResetCount();

            m_Scenario.FirstModeRule.OnUpdate = () => Thread.Sleep(1);
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Scenario.FirstModeRule.OnUpdate = null;
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.InitializeCallCount);
            m_Scenario.ResetFirstGameMode();

            // If rule is stalling during unload, module launches the switchToFallback operation
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();
            m_Scenario.FirstModeSetup.CustomPerformancePolicy = GetTestPerformancePolicy();
            m_Process.SwitchToGameMode(m_Scenario.FirstModeSetup);
            m_Scenario.SimulateUntil(() => m_Process.IsFullyOperational);

            m_Scenario.FirstModeRule.OnUnload = null;
            m_Scenario.SimulateUntil(() => m_Scenario.FirstModeRule.UpdateCallCount > 0);
            m_Process.CurrentGameMode.Unload();
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            Assert.AreEqual(1, m_Scenario.FirstModeRule.UnloadCallCount);
            Assert.AreEqual(1, m_Scenario.SecondModeRule.InitializeCallCount);
        }

        private ExceptionPolicy GetTestExceptionPolicy()
        {
            // Exception behaviours corresponds to module operations
            return new ExceptionPolicy()
            {
                ReactionDuringLoad = OnExceptionBehaviour.UnloadModule,
                ReactionDuringUpdate = OnExceptionBehaviour.ReloadModule,
                ReactionDuringUnload = OnExceptionBehaviour.SwitchToFallback,
                SkipUnloadIfException = true,
                FallbackModule = m_Scenario.SecondModeSetup
            };
        }

        private PerformancePolicy GetTestPerformancePolicy()
        {
            return new PerformancePolicy()
            {
                CheckStallingRules = true,
                InitStallingTimeout = 1,
                UpdateStallingTimeout = 1,
                UnloadStallingTimeout = 1,
                NbWarningsBeforeException = 0,
                MaxFrameDuration = 16
            };
        }
    }
}
