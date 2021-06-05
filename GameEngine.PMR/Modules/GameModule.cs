using GameEngine.Core.FSM;
using GameEngine.Core.FSM.CustomFSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.States;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Modes;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Modules
{
    /// <summary>
    /// A process unit called GameModule used to load and run a coherent set of GameRules defined in a IGameModuleSetup
    /// </summary>
    public class GameModule
    {
        internal const string TAG = "Module";

        /// <summary>
        /// The name of the module, given by the IGameModuleSetup
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state of the module
        /// </summary>
        public GameModuleState State => m_StateMachine.CurrentStateId;

        /// <summary>
        /// If the module is currently in one of its initialization state
        /// </summary>
        public bool IsLoading => State == GameModuleState.Setup || State == GameModuleState.InjectDependencies || State == GameModuleState.InitializeRules;

        /// <summary>
        /// If the module is currently in its default operating state
        /// </summary>
        public bool IsOperational => State == GameModuleState.UpdateRules;

        /// <summary>
        /// If the module is currently in one of its unloading state
        /// </summary>
        public bool IsUnloading => State == GameModuleState.UnloadRules;

        /// <summary>
        /// If the module has finished its lifecycle and is waiting for closure
        /// </summary>
        public bool IsFinished => State == GameModuleState.End;

        /// <summary>
        /// A floating number between 0 and 1 indicating the progression of the loading operations of the module
        /// </summary>
        public float LoadingProgress { get; internal set; }

        /// <summary>
        /// The configuration of the module, set at construction and used to transmit runtime information
        /// </summary|
        public Configuration Configuration { get; private set; }

        internal RulesDictionary Rules;
        internal List<Type> InitUnloadOrder;
        internal List<RuleScheduling> UpdateScheduler;
        internal DependencyProvider DependencyProvider;
        internal ExceptionPolicy ExceptionPolicy;
        internal PerformancePolicy PerformancePolicy;
        internal GameProcess MainProcess;

        internal Action OnFinishLoading;
        internal Action OnFinishUnloading;

        private QueueFSM<GameModuleState> m_StateMachine;
        private bool m_IsPaused;

        internal GameModule(IGameModuleSetup setup, Configuration configuration, GameProcess mainProcess)
        {
            Name = setup.Name;
            Configuration = configuration;
            MainProcess = mainProcess;

            m_IsPaused = false;
            m_StateMachine = new QueueFSM<GameModuleState>($"{Name}FSM",
                new List<FSMState<GameModuleState>>()
                {
                    new StartState(this),
                    new SetupState(this, setup),
                    new InjectDependenciesState(this, mainProcess, null),
                    new InitializeRulesState(this),
                    new UpdateRulesState(this, mainProcess.Time),
                    new UnloadRulesState(this),
                    new EndState(this),
                },
                new List<GameModuleState>() { GameModuleState.Start });
            m_StateMachine.Start();
        }

        /// <summary>
        /// Pause the module. This will freeze all its rules and keep them in the same state until restart
        /// </summary>
        public void Pause()
        {
            Log.Info(TAG, $"Pause module {Name}");
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the module, which will undo the effects of pause
        /// </summary>
        public void Restart()
        {
            Log.Info(TAG, $"Restart module {Name}");
            m_IsPaused = false;
        }

        internal void InnerLoad()
        {
            Log.Info(TAG, $"Load module {Name}");

            m_StateMachine.ClearStateQueue();
            m_StateMachine.EnqueueState(GameModuleState.Setup);
            m_StateMachine.EnqueueState(GameModuleState.InjectDependencies);
            m_StateMachine.EnqueueState(GameModuleState.InitializeRules);
            m_StateMachine.EnqueueState(GameModuleState.UpdateRules);
            m_StateMachine.DequeueState();
        }

        internal void InnerUpdate()
        {
            if (!m_IsPaused)
            {
                m_StateMachine.Update();
            }
        }

        internal void InnerUnload()
        {
            if (State == GameModuleState.End)
                return;

            Log.Info(TAG, $"Unload module {Name}");

            m_StateMachine.ClearStateQueue();
            if (State == GameModuleState.UpdateRules || State == GameModuleState.InitializeRules || State == GameModuleState.UnloadRules)
                m_StateMachine.EnqueueState(GameModuleState.UnloadRules);
            m_StateMachine.EnqueueState(GameModuleState.End);
            m_StateMachine.DequeueState(priority: 100);
        }

        internal void InnerReload()
        {
            if (State == GameModuleState.Start)
                InnerLoad();

            if (State == GameModuleState.End)
                return;

            Log.Info(TAG, $"Reload module {Name}");

            if (State == GameModuleState.Setup || State == GameModuleState.InjectDependencies)
                return;

            m_StateMachine.ClearStateQueue();
            m_StateMachine.EnqueueState(GameModuleState.UnloadRules);
            m_StateMachine.EnqueueState(GameModuleState.InitializeRules);
            m_StateMachine.EnqueueState(GameModuleState.UpdateRules);
            m_StateMachine.DequeueState(priority: 100);
        }

        internal void InnerStop()
        {
            m_StateMachine.Stop();
        }

        internal void InnerQuit()
        {
            Log.Info(TAG, $"Quit module {Name}");

            foreach (KeyValuePair<Type, GameRule> ruleInfo in Rules)
            {
                try
                {
                    ruleInfo.Value.BaseQuit();
                }
                catch (Exception e)
                {
                    Log.Exception(ruleInfo.Value.Name, e);
                }
            }
            InnerStop();
        }

        internal void GoToNextState()
        {
            m_StateMachine.DequeueState();
        }

        internal void ReportLoadingProgress(float progress)
        {
            LoadingProgress = progress;
        }

        internal void OnManagedError()
        {
            try
            {
                MainProcess.SwitchToGameMode((IGameModeSetup)ExceptionPolicy.FallbackModule);
            }
            catch (Exception e)
            {
                Log.Exception(Name, e);
                MainProcess.SwitchToGameMode(null);
            }
        }

        internal bool OnException(OnExceptionBehaviour behaviour)
        {
            switch (behaviour)
            {
                case OnExceptionBehaviour.Continue:
                    return false;
                case OnExceptionBehaviour.SkipFrame:
                    return true;
                case OnExceptionBehaviour.PauseModule:
                    Pause();
                    return true;
                case OnExceptionBehaviour.UnloadModule:
                    MainProcess.SwitchToGameMode(null);
                    return true;
                case OnExceptionBehaviour.ReloadModule:
                    InnerReload();
                    return true;
                case OnExceptionBehaviour.SwitchToFallback:
                    MainProcess.SwitchToGameMode((IGameModeSetup)ExceptionPolicy.FallbackModule);
                    return true;
                case OnExceptionBehaviour.PauseAll:
                    MainProcess.Pause();
                    return true;
                case OnExceptionBehaviour.StopAll:
                    MainProcess.Stop();
                    return true;
                default:
                    return false;
            }
        }
    }
}
