using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules;
using GameEnginesTest.Tools.Mocks.Fakes;
using GameEnginesTest.Tools.Mocks.Stubs;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.PMR
{
    /// <summary>
    /// Component tests for the class GameProcess
    /// <see cref="GameProcess"/>
    /// </summary>
    [TestClass]
    public class GameProcessTest
    {
        FakeTime m_Time;

        public GameProcessTest()
        {
            m_Time = new FakeTime();
        }

        [TestMethod]
        public void PerformCompleteLifeCycle()
        {
            GameProcess process = CreateValidGameProcess(out IGameProcessSetup setup);

            // At creation, process is correctly configured
            Assert.AreEqual(setup.Name, process.Name);
            Assert.AreEqual(m_Time, process.Time);
            Assert.IsFalse(process.IsStarted);
            Assert.IsNull(process.Services);
            Assert.IsNull(process.CurrentGameMode);

            // Start process
            process.Start();
            Assert.IsTrue(process.IsStarted);
            Assert.IsNotNull(process.Services);
            Assert.AreEqual(GameModuleState.Start, process.Services.State);
            Assert.IsNull(process.ServiceProvider);
            Assert.IsNull(process.CurrentGameMode);

            // Run simulation until game services are loaded
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.Services.Orchestrator.IsOperational));
            Assert.AreEqual(GameModuleState.UpdateRules, process.Services.State);
            Assert.IsNotNull(process.ServiceProvider);
            Assert.IsNotNull(process.CurrentGameMode);
            Assert.IsTrue(process.CurrentGameMode.State == GameModuleState.Start || process.CurrentGameMode.State == GameModuleState.Setup);

            // Run simulation until first game mode is loaded
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));
            Assert.AreEqual(GameModuleState.UpdateRules, process.Services.State);
            Assert.AreEqual(GameModuleState.UpdateRules, process.CurrentGameMode.State);

            // Update
            process.Update();

            // Stop process
            process.Stop();
            GameModule gameServices = process.Services;
            GameModule gameMode = process.CurrentGameMode;

            // Run simulation until current game mode is unloaded
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode == null));
            Assert.AreEqual(GameModuleState.UpdateRules, process.Services.State);
            Assert.AreEqual(GameModuleState.End, gameMode.State);

            // Run simulation until game services are unloaded
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.Services == null));
            Assert.AreEqual(GameModuleState.End, gameServices.State);
            Assert.IsNull(process.CurrentGameMode);
            Assert.IsFalse(process.IsStarted);
        }

        [TestMethod]
        public void CanSwitchGameModes()
        {
            GameProcess process = CreateValidGameProcess(out IGameProcessSetup setup);
            process.Start();

            // Initialize process with a custom game mode
            IGameModeSetup customMode = GetCustomGameModeSetup("CustomMode");
            process.SwitchToGameMode(customMode, null);
            process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode != null && process.CurrentGameMode.Orchestrator.IsOperational);
            Assert.AreEqual(setup.GetServiceSetup().Name, process.Services.Name);
            Assert.AreNotEqual(setup.GetFirstGameModes()[0].Name, process.CurrentGameMode.Name);
            Assert.AreEqual(customMode.Name, process.CurrentGameMode.Name);

            // Switch to a new game mode
            IGameModeSetup newMode = GetCustomGameModeSetup("NewMode");
            Configuration newConfig = new Configuration();
            process.SwitchToGameMode(newMode, newConfig);
            process.SimulateOneFrame(m_Time);
            Assert.IsFalse(process.CurrentGameMode.Orchestrator.IsOperational);
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));
            Assert.AreEqual(newMode.Name, process.CurrentGameMode.Name);
            Assert.AreEqual(newConfig, process.CurrentGameMode.Configuration);

            // Prepare the next game modes to come
            IGameModeSetup firstMode = GetCustomGameModeSetup("FirstMode");
            IGameModeSetup secondMode = GetCustomGameModeSetup("SecondMode");
            IGameModeSetup thirdMode = GetCustomGameModeSetup("ThirdMode");
            process.PlanIncomingGameModes(new List<IGameModeSetup>() { firstMode, secondMode, thirdMode }, true);

            // Switch from mode to mode according to the plan
            Assert.IsTrue(process.SwitchToNextGameMode());
            process.SimulateOneFrame(m_Time);
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));
            Assert.AreEqual(firstMode.Name, process.CurrentGameMode.Name);

            Assert.IsTrue(process.SwitchToNextGameMode());
            process.SimulateOneFrame(m_Time);
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));
            Assert.AreEqual(secondMode.Name, process.CurrentGameMode.Name);

            // Clear queue of incoming game modes
            process.PlanIncomingGameModes(new List<IGameModeSetup>(), true);
            Assert.IsFalse(process.SwitchToNextGameMode());

            // Unload current game mode by switching to a null one
            process.SwitchToGameMode(null);
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode == null));
        }

        [TestMethod]
        public void CanBePausedAndRestarted()
        {
            GameProcess process = CreateValidGameProcess(out IGameProcessSetup _);
            process.Start();

            // Pause during the loading of the services
            process.Pause();
            Assert.IsFalse(process.SimulateExecutionUntil(m_Time, () => process.Services.Orchestrator.IsOperational));
            process.Restart();
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.Services.Orchestrator.IsOperational));

            // Pause during the loading of the first game mode
            process.Pause();
            Assert.IsFalse(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));
            process.Restart();
            Assert.IsTrue(process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational));

            // Pause during the main operational phase
            process.Pause();
            process.SwitchToGameMode(GetCustomGameModeSetup("OtherMode"));
            process.SimulateOneFrame(m_Time);
            Assert.IsTrue(process.CurrentGameMode.Orchestrator.IsOperational);

            process.Restart();
            process.SimulateOneFrame(m_Time);
            Assert.IsFalse(process.CurrentGameMode.Orchestrator.IsOperational);

            // Quit the process abruptly
            process.OnQuit();
        }

        [TestMethod]
        public void RegisterConfigurations()
        {
            GameProcess process = CreateValidGameProcess(out IGameProcessSetup setup);

            // Register configurations for the setup services and game modes
            Configuration serviceConfig = new Configuration() { { "serviceParam", "value" } };
            Configuration modeConfig = new Configuration() { { "modeParam", "value" } };
            Assert.ThrowsException<ArgumentException>(() => process.GetModuleConfiguration(typeof(StubGameRulePack)));
            process.SetModuleConfiguration(typeof(StubGameServiceSetup), serviceConfig);
            process.SetModuleConfiguration(typeof(StubGameModeSetup), modeConfig);

            // Complete loading and check configurations are correct
            process.Start();
            process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode != null && process.CurrentGameMode.Orchestrator.IsOperational);
            Assert.AreEqual(serviceConfig, process.Services.Configuration);
            Assert.AreEqual(modeConfig, process.CurrentGameMode.Configuration);

            // Retrieve a module configuration
            Assert.ThrowsException<ArgumentException>(() => process.GetModuleConfiguration(typeof(StubGameRule)));
            Assert.IsNull(process.GetModuleConfiguration(typeof(StubGameSubmoduleSetup)));
            Assert.AreEqual(serviceConfig, process.GetModuleConfiguration(typeof(StubGameServiceSetup)));

            // During further loads, use specified configuration instead of preregistered one (if given)
            Configuration customConfig = new Configuration() { { "customParam", "value" } };
            process.SwitchToGameMode(setup.GetFirstGameModes()[0], customConfig);
            process.SimulateOneFrame(m_Time);
            process.SimulateExecutionUntil(m_Time, () => process.CurrentGameMode.Orchestrator.IsOperational);
            Assert.AreEqual(customConfig, process.CurrentGameMode.Configuration);
        }

        private GameProcess CreateValidGameProcess(out IGameProcessSetup setup)
        {
            // Create an empty GameService setup
            StubGameServiceSetup servicesSetup = new StubGameServiceSetup();
            servicesSetup.CustomRules = new List<GameRule>();

            // Create an empty GameMode setup
            StubGameModeSetup modeSetup = new StubGameModeSetup();
            modeSetup.CustomRules = new List<GameRule>();

            // Create a process with those GameService and GameMode setups
            StubGameProcessSetup processSetup = new StubGameProcessSetup();
            processSetup.CustomServiceSetup = servicesSetup;
            processSetup.CustomGameModes = new List<IGameModeSetup>() { modeSetup };
            setup = processSetup;

            return new GameProcess(processSetup, m_Time);
        }

        private IGameModeSetup GetCustomGameModeSetup(string name)
        {
            // Create an empty GameMode setup with a custom name
            StubGameModeSetup modeSetup = new StubGameModeSetup();
            modeSetup.CustomRules = new List<GameRule>();
            modeSetup.CustomName = name;

            return modeSetup;
        }
    }
}
