using GameEngine.PSMR.Modes;
using GameEngine.PSMR.Rules.Dependencies;
using GameEngine.PSMR.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace GameEngine.PSMR.Process
{
    /// <summary>
    /// A process that simulates the lifecycle of a game whose caracteristics (rules and services) are defined in a given GameProcessSetup
    /// </summary>
    public class GameProcess
    {
        /// <summary>
        /// The name of the GameProcess
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A reference to the interface giving time information concerning the GameProcess
        /// </summary>
        public IProcessTime Time { get; private set; }

        /// <summary>
        /// The ServiceGameMode running the GameServices of the game, i.e rules that are running and accessible throughout the life of the process
        /// </summary>
        public GameMode ServiceMode { get; internal set; }

        /// <summary>
        /// The Current GameMode running a set of temporary GameRules for the game
        /// </summary>
        public GameMode CurrentGameMode { get; internal set; }

        /// <summary>
        /// Inform if the GameProcess is running, that is to say currently performing operations
        /// </summary>
        public bool IsRunning => ServiceMode != null;

        internal DependencyProvider ServiceProvider;

        private IServiceSetup m_ServiceSetup;
        private IGameModeSetup m_NextGameModeSetup;
        private IConfiguration m_NextGameModeConfig;
        private Queue<IGameModeSetup> m_GameModesToCome;
        private bool m_IsPaused;
        private bool m_IsStopping;

        /// <summary>
        /// Constructor of the GameProcess
        /// </summary>
        /// <param name="setup">custom setup defining game characteristics</param>
        /// <param name="time">interface for accessing time information from within the game process</param>
        public GameProcess(IGameProcessSetup setup, IProcessTime time)
        {
            Name = $"{setup.Name}Process";
            Time = time;
            m_ServiceSetup = setup.GetServiceSetup();
            m_GameModesToCome = new Queue<IGameModeSetup>(setup.GetFirstGameModes());
            m_GameModesToCome.TryDequeue(out m_NextGameModeSetup);
            CheckGameModeValidity(m_NextGameModeSetup);
            m_IsPaused = false;
        }

        /// <summary>
        /// Start the GameProcess
        /// </summary>
        public void Start()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (IsRunning)
                throw new InvalidOperationException($"Start() should be called when process {Name} is not already running");
#endif
            ServiceMode = new GameMode(m_ServiceSetup, null, this);
            ServiceMode.Start();
        }

        /// <summary>
        /// Update the GameProcess
        /// </summary>
        public void Update()
        {
            if (!m_IsPaused && ServiceMode != null)
            {
                ServiceMode.Update();

                if (ServiceMode.IsOperational)
                {
                    if (CurrentGameMode != null)
                    {
                        CurrentGameMode.Update();

                        if (CurrentGameMode.IsFinished)
                        {
                            CurrentGameMode.Stop();
                            CurrentGameMode = null;
                        }
                    }

                    if (CurrentGameMode == null)
                    {
                        if (m_NextGameModeSetup != null)
                        {
                            CurrentGameMode = new GameMode(m_NextGameModeSetup, m_NextGameModeConfig, this);
                            CurrentGameMode.Start();
                            m_NextGameModeSetup = null;
                            m_NextGameModeConfig = null;
                        }
                        else if (m_IsStopping)
                        {
                            ServiceMode.Unload();
                        }
                    }
                }
                else if (ServiceMode.IsFinished)
                {
                    ServiceMode.Stop();
                    ServiceMode = null;
                }
            }
        }

        /// <summary>
        /// Pause the GameProcess, which means ignoring all updates
        /// </summary>
        public void Pause()
        {
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the GameProcess, which means canceling the pause
        /// </summary>
        public void Restart()
        {
            m_IsPaused = false;
        }

        /// <summary>
        /// Stop the GameProcess
        /// The effects of the stop are not immediate, since it requires potentially long unloading operations
        /// Check the IsRunning property to be informed when the GameProcess is really stopped
        /// </summary>
        public void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (!IsRunning)
                throw new InvalidOperationException($"Stop() should be called when process {Name} is running");
#endif
            if (!m_IsStopping)
            {
                m_IsStopping = true;
                m_NextGameModeSetup = null;
                m_NextGameModeConfig = null;

                if (CurrentGameMode == null)
                    ServiceMode.Unload();
                else
                    CurrentGameMode.Unload();
            }
        }

        /// <summary>
        /// Call OnQuit() for a quick and immediate shut down of the GameProcess
        /// This method executes synchronous operations, that will be finished by the end of the call
        /// </summary>
        public void OnQuit()
        {
            CurrentGameMode?.OnQuit();
            ServiceMode?.OnQuit();
        }

        /// <summary>
        /// Switch to a given GameMode : unload the current mode and then load the new one
        /// </summary>
        /// <param name="setup">setup of the new GameMode to load</param>
        /// <param name="configuration">initial configuration of the GameMode, used to transmit information between GameModes at runtime</param>
        public void SwitchToGameMode(IGameModeSetup setup, IConfiguration configuration = null)
        {
            if (!m_IsStopping)
            {
                CheckGameModeValidity(setup);

                CurrentGameMode?.Unload();
                m_NextGameModeSetup = setup;
                m_NextGameModeConfig = configuration;
            }
        }

        /// <summary>
        /// Switch to the next planned GameMode in the list that was prepared in advance
        /// </summary>
        /// <param name="configuration">initial configuration of the GameMode, used to transmit information between GameModes at runtime</param>
        /// <returns>If there was a next GameMode to switch to</returns>
        public bool SwitchToNextGameMode(IConfiguration configuration = null)
        {
            if (m_GameModesToCome.TryDequeue(out IGameModeSetup setup))
            {
                SwitchToGameMode(setup, configuration);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enqueue an anticipated list of GameModes that the process will normally have to pass through
        /// </summary>
        /// <param name="gameModes">The ordered list of GameModes to be run in the future</param>
        /// <param name="replace">If the new given list of GameModes should replace the existing one</param>
        public void PrepareIncomingGameModes(List<IGameModeSetup> gameModes, bool replace)
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

        private void CheckGameModeValidity(IGameModeSetup setup)
        {
            if (setup.RequiredServiceSetup != m_ServiceSetup.GetType())
                throw new ArgumentException($"Cannot load GameMode {setup.Name} because it requires a service setup {setup.RequiredServiceSetup} " +
                    $"that is different from the current one ({m_ServiceSetup.GetType()})", "setup.RequiredServiceSetup");
        }
    }
}
