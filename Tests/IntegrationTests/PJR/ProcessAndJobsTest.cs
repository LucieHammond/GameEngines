using GameEngine.PJR.Jobs;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Modes;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Mocks;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.IntegrationTests.PJR
{
    /// <summary>
    /// Integration tests focusing on the interactions between GameProcess and GameJobs
    /// </summary>
    [TestClass]
    public class ProcessAndJobsTest
    {
        MockProcessTime m_Time;
        private GameProcess m_Process;

        [TestInitialize]
        public void Initialize()
        {
            m_Time = new MockProcessTime();

            // Create an empty process with no services and no rules (interactions with rules are not tested here)
            DummyServiceSetup servicesSetup = new DummyServiceSetup();
            servicesSetup.CustomServices = new List<GameService>();

            DummyGameModeSetup modeSetup = new DummyGameModeSetup();
            modeSetup.CustomRules = new List<GameRule>();

            DummyGameProcessSetup processSetup = new DummyGameProcessSetup();
            processSetup.CustomServiceSetup = servicesSetup;
            processSetup.CustomGameModes = new List<IGameModeSetup>() { modeSetup };
            m_Process = new GameProcess(processSetup, m_Time);
        }

        [TestMethod]
        public void ProcessIsStarted_JobsAreLoaded()
        {
            // Load service handler job
            m_Process.Start();
            Assert.AreEqual(GameJobState.Setup, m_Process.ServiceHandler.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.DependencyInjection, m_Process.ServiceHandler.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.InitializeRules, m_Process.ServiceHandler.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.UpdateRules, m_Process.ServiceHandler.State);

            // Load game mode job
            Assert.AreEqual(GameJobState.Setup, m_Process.CurrentGameMode.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.DependencyInjection, m_Process.CurrentGameMode.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.InitializeRules, m_Process.CurrentGameMode.State);
            RunNextFrame();
            Assert.AreEqual(GameJobState.UpdateRules, m_Process.CurrentGameMode.State);
        }

        [TestMethod]
        public void ProcessIsStopped_JobsAreUnloaded()
        {
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);

            // Unload game mode job
            m_Process.Stop();
            RunNextFrame();
            Assert.AreEqual(GameJobState.UnloadRules, m_Process.CurrentGameMode.State);
            RunNextFrame();
            Assert.IsNull(m_Process.CurrentGameMode);

            // Unoad service handler job
            RunNextFrame();
            Assert.AreEqual(GameJobState.UnloadRules, m_Process.ServiceHandler.State);
            RunNextFrame();
            Assert.IsNull(m_Process.ServiceHandler);
        }

        [TestMethod]
        public void JobAskForUnload_ProcessUnloadJob()
        {
            // If the job is a GameMode -> switch to fallback mode
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);
            
            DummyGameModeSetup fallbackSetup = new DummyGameModeSetup();
            fallbackSetup.CustomRules = new List<GameRule>();
            fallbackSetup.CustomName = "Fallback";
            m_Process.CurrentGameMode.ExceptionPolicy.FallbackMode = fallbackSetup;
            
            m_Process.CurrentGameMode.AskUnload();
            RunNextFrame();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);
            Assert.AreEqual("FallbackMode", m_Process.CurrentGameMode.Name);

            // If the job is a ServiceHandler -> unload game mode and service mode
            Initialize();
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);

            m_Process.ServiceHandler.AskUnload();
            RunNextFrame();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode == null);
            RunNextFrame();
            Assert.IsTrue(m_Process.ServiceHandler.IsUnloading);
        }

        [TestMethod]
        public void JobCatchException_ProcessApplyReaction()
        {
            // When OnExceptionBehaviour = UnloadJob
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);
            m_Process.CurrentGameMode.OnException(OnExceptionBehaviour.UnloadJob);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode == null);
            RunNextFrame();
            Assert.IsFalse(m_Process.ServiceHandler.IsUnloading);

            // When OnExceptionBehaviour = PauseAll
            Initialize();
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);
            m_Process.CurrentGameMode.OnException(OnExceptionBehaviour.PauseAll);
            m_Process.Stop();
            RunNextFrame();
            Assert.IsFalse(m_Process.CurrentGameMode.IsUnloading);
            m_Process.Restart();
            RunNextFrame();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);

            // When OnExceptionBehaviour = StopAll
            Initialize();
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational);
            m_Process.CurrentGameMode.OnException(OnExceptionBehaviour.StopAll);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode == null);
            RunNextFrame();
            Assert.IsTrue(m_Process.ServiceHandler.IsUnloading);
        }

        private void RunNextFrame()
        {
            m_Process.Update();
            m_Time.GoToNextFrame();
        }
    }
}
