using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Process.Structure;
using GameEngine.PMR.Rules.Dependencies;
using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Process
{
    /// <summary>
    /// A GameProcess that simulates the lifecycle of a game whose caracteristics (services and modes) are defined in a given IGameProcessSetup
    /// </summary>
    public class GameProcess
    {
        internal const string TAG = "Process";

        /// <summary>
        /// The name of the process
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A reference to the interface giving time information concerning the process
        /// </summary>
        public ITime Time { get; private set; }

        /// <summary>
        /// The GameService module running the services of the game, i.e rules that are operational and accessible throughout the life of the process
        /// </summary>
        public GameModule Services => m_GameServiceOrchestrator.CurrentModule;

        /// <summary>
        /// The current GameMode module running a consistent set of temporary rules for the game
        /// </summary>
        public GameModule CurrentGameMode => m_GameModeOrchestrator.CurrentModule;

        /// <summary>
        /// If the process has been started
        /// </summary>
        public bool IsStarted => Services != null;

        /// <summary>
        /// If the process modules (game services and game mode) are completely loaded and currently running
        /// </summary>
        public bool IsFullyOperational => m_GameServiceOrchestrator.IsOperational && m_GameModeOrchestrator.IsOperational;

        internal DependencyProvider ServiceProvider => Services?.DependencyProvider;

        private Orchestrator m_GameServiceOrchestrator;
        private Orchestrator m_GameModeOrchestrator;
        private IGameServiceSetup m_ServiceSetup;
        private Queue<IGameModeSetup> m_GameModesToCome;
        private Dictionary<Type, Configuration> m_Configurations;
        private bool m_IsPaused;

        /// <summary>
        /// Constructor of the GameProcess
        /// </summary>
        /// <param name="setup">Custom setup defining game characteristics</param>
        /// <param name="time">Interface for accessing time information from within the game process</param>
        public GameProcess(IGameProcessSetup setup, ITime time)
        {
            Name = setup.Name;
            Time = time;

            m_GameServiceOrchestrator = new Orchestrator("GameServices", this, null);
            m_GameModeOrchestrator = new Orchestrator("GameMode", this, null);
            m_ServiceSetup = setup.GetServiceSetup();
            m_GameModesToCome = new Queue<IGameModeSetup>(setup.GetFirstGameModes());
            m_Configurations = new Dictionary<Type, Configuration>();
            m_IsPaused = false;
        }

        /// <summary>
        /// Start the process, which means begin to load all game rules
        /// </summary>
        public void Start()
        {
            Log.Info(TAG, $"Start process {Name}");

            m_GameServiceOrchestrator.LoadModule(m_ServiceSetup);
            m_GameServiceOrchestrator.OnOperational = () => SwitchToNextGameMode();
        }

        /// <summary>
        /// Update the process, which means update all game rules according to their schedule
        /// </summary>
        public void Update()
        {
            if (!m_IsPaused)
            {
                m_GameServiceOrchestrator.Update();

                if (m_GameServiceOrchestrator.IsOperational)
                    m_GameModeOrchestrator.Update();
            }
        }

        /// <summary>
        /// Perform the fixed update of the process, which means call the fixed update of all game rules according to their schedule
        /// </summary>
        public void FixedUpdate()
        {
            if (!m_IsPaused)
            {
                m_GameServiceOrchestrator.FixedUpdate();

                if (m_GameServiceOrchestrator.IsOperational)
                    m_GameModeOrchestrator.FixedUpdate();
            }
        }

        /// <summary>
        /// Perform the late update of the process, which means call the late update of all game rules according to their schedule
        /// </summary>
        public void LateUpdate()
        {
            if (!m_IsPaused)
            {
                m_GameServiceOrchestrator.LateUpdate();

                if (m_GameServiceOrchestrator.IsOperational)
                    m_GameModeOrchestrator.LateUpdate();
            }
        }

        /// <summary>
        /// Pause the process, which means ignore all subsequent updates
        /// </summary>
        public void Pause()
        {
            Log.Info(TAG, $"Pause process {Name}");
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the process, which means cancel the pause and resume the updates
        /// </summary>
        public void Restart()
        {
            Log.Info(TAG, $"Restart process {Name}");
            m_IsPaused = false;
        }

        /// <summary>
        /// Stop the process, which means begin to unload all game rules
        /// </summary>
        public void Stop()
        {
            Log.Info(TAG, $"Stop process {Name}");

            if (Services == null)
            {
                return;
            }
            else if (CurrentGameMode == null)
            {
                m_GameServiceOrchestrator.UnloadModule();
            }
            else
            {
                m_GameModeOrchestrator.UnloadModule();
                m_GameModeOrchestrator.OnTerminated += () => m_GameServiceOrchestrator.UnloadModule();
            }
        }

        /// <summary>
        /// Quit the process, which means perform a quick and immediate shut down of all operations in one synchronous single call
        /// </summary>
        public void OnQuit()
        {
            Log.Info(TAG, $"Quit process {Name}");

            m_GameModeOrchestrator.OnQuit();
            m_GameServiceOrchestrator.OnQuit();
        }

        /// <summary>
        /// Register a default configuration for a specific module type that is likely to be loaded during the game
        /// </summary>
        /// <param name="setupType">The type of the setup defining the module</param>
        /// <param name="configuration">The configuration to register for this type of module</param>
        public void SetModuleConfiguration(Type setupType, Configuration configuration)
        {
            if (!typeof(IGameModuleSetup).IsAssignableFrom(setupType))
                throw new ArgumentException($"The given type {setupType} is not a setup type, implementing IGameModuleSetup", nameof(setupType));

            m_Configurations[setupType] = configuration;
        }

        /// <summary>
        /// Retrieve the default pre-registered configuration for a specific module type, if exists
        /// </summary>
        /// <param name="setupType">The type of the setup defining the module</param>
        /// <returns>The registered configuration for this type of module (null if not found)</returns>
        public Configuration GetModuleConfiguration(Type setupType)
        {
            if (!typeof(IGameModuleSetup).IsAssignableFrom(setupType))
                throw new ArgumentException($"The given type {setupType} is not a setup type, implementing IGameModuleSetup", nameof(setupType));

            if (m_Configurations.TryGetValue(setupType, out Configuration config))
                return config;
            return null;
        }

        /// <summary>
        /// Switch to a given game mode, i.e unload the current mode and then load the new one
        /// </summary>
        /// <param name="setup">The setup defining the new mode to load</param>
        /// <param name="configuration">The initial configuration of the mode. If not set, a pre-registered configuration will be used</param>
        public void SwitchToGameMode(IGameModeSetup setup, Configuration configuration = null)
        {
            if (m_GameServiceOrchestrator.IsOperational)
            {
                if (setup == null && CurrentGameMode == null)
                    return;

                if (setup == null)
                    m_GameModeOrchestrator.UnloadModule();
                else if (CurrentGameMode == null)
                    m_GameModeOrchestrator.LoadModule(setup, configuration);
                else
                    m_GameModeOrchestrator.SwitchToModule(setup, configuration);
            }
            else
            {
                m_GameServiceOrchestrator.OnOperational = () => SwitchToGameMode(setup, configuration);
            }
        }

        /// <summary>
        /// Switch to the next planned game mode in the list that has been prepared in advance
        /// </summary>
        /// <returns>If the switch operation was really executed (the list of modes was not empty)</returns>
        public bool SwitchToNextGameMode()
        {
            if (m_GameModesToCome.Count > 0)
            {
                SwitchToGameMode(m_GameModesToCome.Dequeue());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enqueue an anticipated list of game modes that the process will normally have to pass through
        /// </summary>
        /// <param name="gameModes">The ordered list of modes to be run in the future</param>
        /// <param name="replace">If the new given list of modes should replace the existing one</param>
        public void PlanIncomingGameModes(List<IGameModeSetup> gameModes, bool replace)
        {
            if (replace)
                m_GameModesToCome = new Queue<IGameModeSetup>(gameModes);
            else
            {
                foreach (IGameModeSetup mode in gameModes)
                {
                    m_GameModesToCome.Enqueue(mode);
                }
            }
        }

        internal void ResetGameMode()
        {
            m_GameModeOrchestrator.OnQuit();
            m_GameModeOrchestrator.CurrentModule = null;
        }
    }
}
