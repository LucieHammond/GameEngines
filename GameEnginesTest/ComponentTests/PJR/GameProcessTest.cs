using GameEngine.Core.Model;
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

namespace GameEnginesTest.ComponentTests.PJR
{
    /// <summary>
    /// Component tests for the class GameProcess
    /// <see cref="GameProcess"/>
    /// </summary>
    [TestClass]
    public class GameProcessTest
    {
        MockProcessTime m_Time;
        GameProcess m_Process;

        public GameProcessTest()
        {
            m_Time = new MockProcessTime();

            // Create an empty process with no services and no rules (interactions with rules are not tested here)
            DummyGameServiceSetup servicesSetup = new DummyGameServiceSetup();
            servicesSetup.CustomServices = new List<GameRule>();

            DummyGameModeSetup modeSetup = new DummyGameModeSetup();
            modeSetup.CustomRules = new List<GameRule>();

            DummyGameProcessSetup processSetup = new DummyGameProcessSetup();
            processSetup.CustomServiceSetup = servicesSetup;
            processSetup.CustomGameModes = new List<IGameModeSetup>() { modeSetup };
            m_Process = new GameProcess(processSetup, m_Time);
        }

        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            // Process is created -> retrievable parameters are correct
            Assert.AreEqual("TestProcess", m_Process.Name);
            Assert.AreEqual(m_Time, m_Process.Time);
            Assert.IsFalse(m_Process.IsRunning);
            Assert.IsNull(m_Process.ServiceHandler);
            Assert.IsNull(m_Process.CurrentGameMode);

            // Start process
            m_Process.Start();
            Assert.IsTrue(m_Process.IsRunning);
            Assert.IsNotNull(m_Process.ServiceHandler);
            Assert.IsNull(m_Process.ServiceProvider);

            // Load services and first game mode
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler.IsOperational));
            Assert.IsNotNull(m_Process.ServiceProvider);
            Assert.IsNotNull(m_Process.CurrentGameMode);
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode.IsOperational));
            m_Process.Update();

            // Srop process : Unload game mode and services
            m_Process.Stop();
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode == null));
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler == null));
            Assert.IsTrue(!m_Process.IsRunning);
        }

        [TestMethod]
        public void CanSwitchGameMode()
        {
            // Start process and load first game mode Test
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler.IsOperational);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode.IsOperational);
            Assert.AreEqual("TestMode", m_Process.CurrentGameMode.Name);

            // Switch to new GameMode Test2
            DummyGameModeSetup modeSetup = CreateEmptyGameModeSetup("TestBis");
            Configuration config = new Configuration();
            m_Process.SwitchToGameMode(modeSetup, config);
            RunNextFrame();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational));
            Assert.AreEqual("TestBisMode", m_Process.CurrentGameMode.Name);
            Assert.AreEqual(config, m_Process.CurrentGameMode.Configuration);

            // Unload current game mode (replace with null game mode)
            m_Process.SwitchToGameMode(null);
            RunNextFrame();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode == null));

            m_Process.Stop();
        }

        [TestMethod]
        public void AnticipateGameModesToLoad()
        {
            // Start process and load first game mode
            m_Process.Start();
            m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler.IsOperational);
            m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode.IsOperational);

            // Prepare next modes to load
            DummyGameModeSetup firstMode = CreateEmptyGameModeSetup("First");
            DummyGameModeSetup secondMode = CreateEmptyGameModeSetup("Second");
            DummyGameModeSetup thirdMode = CreateEmptyGameModeSetup("Third");
            m_Process.PrepareIncomingGameModes(new List<IGameModeSetup>() { firstMode, secondMode, thirdMode }, false);

            // Switch between modes
            Configuration config = new Configuration();
            Assert.IsTrue(m_Process.SwitchToNextGameMode(config));
            RunNextFrame();
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational));
            Assert.AreEqual("FirstMode", m_Process.CurrentGameMode.Name);
            Assert.AreEqual(config, m_Process.CurrentGameMode.Configuration);

            Assert.IsTrue(m_Process.SwitchToNextGameMode());
            RunNextFrame();
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode != null && m_Process.CurrentGameMode.IsOperational));
            Assert.AreEqual("SecondMode", m_Process.CurrentGameMode.Name);

            // Clear queue of incoming modes by replacing the old one with empty queue
            m_Process.PrepareIncomingGameModes(new List<IGameModeSetup>(), true);
            Assert.IsFalse(m_Process.SwitchToNextGameMode());
        }

        [TestMethod]
        public void CanBePausedAndRestarted()
        {
            m_Process.Start();

            // Pause during the loading of the services
            m_Process.Pause();
            Assert.IsFalse(m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler.IsOperational, 5));
            m_Process.Restart();
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.ServiceHandler.IsOperational, 5));

            // Pause during the loading of the first game mode
            m_Process.Pause();
            Assert.IsFalse(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode.IsOperational, 5));
            m_Process.Restart();
            Assert.IsTrue(m_Process.SimulateExecutionUntil(() => m_Process.CurrentGameMode.IsOperational, 5));

            // Pause before unloading the current game mode
            m_Process.Pause();
            m_Process.Stop();
            m_Process.Update();
            Assert.IsFalse(m_Process.CurrentGameMode.IsUnloading);
            m_Process.Restart();
            m_Process.Update();
            Assert.IsTrue(m_Process.CurrentGameMode.IsUnloading);
        }

        private DummyGameModeSetup CreateEmptyGameModeSetup(string name = null)
        {
            DummyGameModeSetup modeSetup = new DummyGameModeSetup();
            modeSetup.CustomRules = new List<GameRule>();
            modeSetup.CustomName = name;
            return modeSetup;
        }

        private void RunNextFrame()
        {
            m_Process.Update();
            m_Time.GoToNextFrame();
        }
    }
}
