using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Scenarios;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GameEnginesTest.IntegrationTests.PMR
{
    /// <summary>
    /// Integration tests focusing on the interactions between <see cref="GameProcess"/> and <see cref="GameModule"/>
    /// </summary>
    [TestClass]
    public class ProcessToModuleTest
    {
        private PMRScenario m_Scenario;
        private GameProcess m_Process;

        public ProcessToModuleTest()
        {
            m_Scenario = new PMRScenario();
            m_Process = m_Scenario.Process;
        }

        [TestMethod]
        public void ProcessPerformsOperation_ModuleChangesState()
        {
            // Start operation
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.Services != null);
            m_Scenario.SimulateUntil(() => m_Process.Services.State == GameModuleState.Configure);
            m_Scenario.SimulateUntil(() => m_Process.Services.State == GameModuleState.InjectDependencies);
            m_Scenario.SimulateUntil(() => m_Process.Services.State == GameModuleState.InitializeRules);
            m_Scenario.SimulateUntil(() => m_Process.Services.State == GameModuleState.UpdateRules);
            GameModule serviceModule = m_Process.Services;

            m_Scenario.SimulateUntil(() => m_Process.IsServiceOperational);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode != null);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.Configure);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InjectDependencies);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InitializeRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            GameModule firstModule = m_Process.CurrentGameMode;

            // SwitchMode Operation
            m_Process.SwitchToNextGameMode();
            m_Scenario.SimulateUntil(() => firstModule.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => firstModule.State == GameModuleState.End);
            Assert.AreNotEqual(firstModule, m_Process.CurrentGameMode);

            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.Configure);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InjectDependencies);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InitializeRules);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.UpdateRules);
            GameModule secondModule = m_Process.CurrentGameMode;

            // Stop operation
            m_Process.Stop();
            m_Scenario.SimulateUntil(() => secondModule.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => secondModule.State == GameModuleState.End);
            Assert.IsNull(m_Process.CurrentGameMode);

            m_Scenario.SimulateUntil(() => serviceModule.State == GameModuleState.UnloadRules);
            m_Scenario.SimulateUntil(() => serviceModule.State == GameModuleState.End);
            Assert.IsNull(m_Process.Services);
        }

        [TestMethod]
        public void ModuleSetupIsInvalid_ProcessLaunchesNewOperation()
        {
            // Setup is invalid because StubGameRuleTer has a required dependency on StubGameRuleBis (missing)
            m_Scenario.FirstModeSetup.CustomRules = new List<GameRule>() { new StubGameRuleTer() };
            m_Scenario.FirstModeSetup.CustomExceptionPolicy = GetTestExceptionPolicy();

            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.IsServiceOperational);

            // Since the setup problem is detected during load, process launches stop operation
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode != null);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.Configure);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode.State == GameModuleState.InjectDependencies);
            GameModule gameMode = m_Process.CurrentGameMode;

            m_Scenario.SimulateUntil(() => gameMode.State == GameModuleState.End);
            m_Scenario.SimulateUntil(() => m_Process.CurrentGameMode == null);
            m_Scenario.SimulateUntil(() => m_Process.Services == null);
        }

        [TestMethod]
        public void ModuleFailsRequirements_ProcessStopsOperation()
        {
            m_Process.Start();
            m_Scenario.SimulateUntil(() => m_Process.IsGameModeOperational);

            // If mode setup requirements are not met, process log error and mode is not loaded
            m_Scenario.SecondModeSetup.CustomRequiredService = typeof(StubGameSubmoduleSetup);
            AssertUtils.LogError(() =>
            {
                m_Process.SwitchToGameMode(m_Scenario.SecondModeSetup);
            },
            GameProcess.TAG);
            m_Scenario.SimulateFrames(5);
            Assert.AreEqual(m_Process.CurrentGameMode, m_Scenario.FirstModeRule.Module);

            // If submodule setup requirements are not met, process log error and submodule is not loaded
            m_Scenario.SubmoduleSetup.CustomRequiredService = null;
            m_Scenario.SubmoduleSetup.CustomRequiredParent = typeof(StubGameSubmoduleSetup);
            AssertUtils.LogError(() =>
            {
                m_Process.CurrentGameMode.LoadSubmodule(m_Scenario.SubmoduleCategory, m_Scenario.SubmoduleSetup);
            },
            GameProcess.TAG);
            Assert.IsNull(m_Process.CurrentGameMode.GetSubmodule(m_Scenario.SubmoduleCategory));
        }

        private ExceptionPolicy GetTestExceptionPolicy()
        {
            // Exception behaviours corresponds to process operations
            return new ExceptionPolicy()
            {
                ReactionDuringLoad = OnExceptionBehaviour.StopAll,
                ReactionDuringUpdate = OnExceptionBehaviour.PauseAll,
                ReactionDuringUnload = OnExceptionBehaviour.StopAll,
                SkipUnloadIfException = true,
                FallbackModule = m_Scenario.SecondModeSetup
            };
        }
    }
}
