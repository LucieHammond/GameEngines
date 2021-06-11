using GameEngine.Core.FSM;
using GameEngine.Core.FSM.CustomFSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.States;
using GameEngine.PMR.Process.Orchestration;
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
        /// The type of the module setup, that serves as an identifier
        /// </summary>
        public Type Id { get; private set; }

        /// <summary>
        /// The name of the module, given by the IGameModuleSetup
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state of the module
        /// </summary>
        public GameModuleState State => m_StateMachine.CurrentStateId;

        /// <summary>
        /// The state of orchestrator that manages the transitions of the module
        /// </summary>
        public OrchestratorState OrchestrationState => Orchestrator.State;

        /// <summary>
        /// The configuration of the module, set at construction and used to transmit runtime information
        /// </summary|
        public Configuration Configuration { get; private set; }

        internal RulesDictionary Rules;
        internal List<Type> InitUnloadOrder;
        internal List<RuleScheduling> UpdateScheduler;
        internal List<RuleScheduling> FixedUpdateScheduler;
        internal List<RuleScheduling> LateUpdateScheduler;
        internal ExceptionPolicy ExceptionPolicy;
        internal PerformancePolicy PerformancePolicy;
        internal DependencyProvider DependencyProvider;
        internal Orchestrator Orchestrator;

        internal Action OnFinishLoading;
        internal Action OnFinishUnloading;

        private QueueFSM<GameModuleState> m_StateMachine;
        private bool m_IsPaused;

        internal GameModule(IGameModuleSetup setup, Configuration configuration, Orchestrator orchestrator)
        {
            Id = setup.GetType();
            Name = setup.Name;
            Configuration = configuration;
            Orchestrator = orchestrator;
            Rules = new RulesDictionary();

            m_IsPaused = false;
            m_StateMachine = new QueueFSM<GameModuleState>($"{Name}FSM",
                new List<FSMState<GameModuleState>>()
                {
                    new StartState(this),
                    new SetupState(this, setup),
                    new InjectDependenciesState(this, orchestrator.MainProcess, orchestrator.Parent?.CurrentModule),
                    new InitializeRulesState(this),
                    new UpdateRulesState(this, orchestrator.MainProcess.Time),
                    new UnloadRulesState(this),
                    new EndState(this),
                },
                new List<GameModuleState>() { GameModuleState.Start });
            m_StateMachine.Start();
        }

        internal void Update()
        {
            if (!m_IsPaused)
                m_StateMachine.Update();
        }

        internal void FixedUpdate()
        {
            if (!m_IsPaused)
                m_StateMachine.FixedUpdate();
        }

        internal void LateUpdate()
        {
            if (!m_IsPaused)
                m_StateMachine.LateUpdate();
        }

        internal void Destroy()
        {
            m_StateMachine.Stop();
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

        internal void InnerUnload()
        {
            if (State == GameModuleState.UnloadRules || State == GameModuleState.End)
                return;

            Log.Info(TAG, $"Unload module {Name}");

            m_StateMachine.ClearStateQueue();
            if (State == GameModuleState.UpdateRules || State == GameModuleState.InitializeRules)
                m_StateMachine.EnqueueState(GameModuleState.UnloadRules);
            m_StateMachine.EnqueueState(GameModuleState.End);
            m_StateMachine.DequeueState(priority: 100);
        }

        internal void InnerReload()
        {
            if (State == GameModuleState.Start)
                InnerLoad();

            if (State == GameModuleState.Setup || State == GameModuleState.InjectDependencies || State == GameModuleState.End)
                return;

            Log.Info(TAG, $"Reload module {Name}");

            m_StateMachine.ClearStateQueue();
            m_StateMachine.EnqueueState(GameModuleState.UnloadRules);
            m_StateMachine.EnqueueState(GameModuleState.InitializeRules);
            m_StateMachine.EnqueueState(GameModuleState.UpdateRules);
            m_StateMachine.DequeueState(priority: 100);
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
            Destroy();
        }

        internal void GoToNextState()
        {
            m_StateMachine.DequeueState();
        }

        internal void ReportLoadingProgress(float progress)
        {
            Orchestrator.CurrentTransition?.SetDefaultProgress(progress);
        }

        internal void OnManagedError()
        {
            if (ExceptionPolicy.FallbackModule == null)
                Orchestrator.UnloadModule();
            else
                Orchestrator.SwitchToModule(ExceptionPolicy.FallbackModule);
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
                    Orchestrator.UnloadModule();
                    return true;
                case OnExceptionBehaviour.ReloadModule:
                    Orchestrator.ReloadModule();
                    return true;
                case OnExceptionBehaviour.SwitchToFallback:
                    Orchestrator.SwitchToModule(ExceptionPolicy.FallbackModule);
                    return true;
                case OnExceptionBehaviour.PauseAll:
                    Orchestrator.MainProcess.Pause();
                    return true;
                case OnExceptionBehaviour.StopAll:
                    Orchestrator.MainProcess.Stop();
                    return true;
                default:
                    return false;
            }
        }
    }
}
