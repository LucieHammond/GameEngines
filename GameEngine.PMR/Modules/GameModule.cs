using GameEngine.Core.FSM;
using GameEngine.Core.FSM.CustomFSM;
using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.States;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Services;
using GameEngine.PMR.Rules;
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
        /// <summary>
        /// The name of the module, based on the name of the IGameModuleSetup
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state of the module
        /// </summary>
        public GameModuleState State => m_StateMachine.CurrentStateId;

        /// <summary>
        /// If the module is currently in one of its initialization state
        /// </summary>
        public bool IsLoading => State == GameModuleState.Setup || State == GameModuleState.DependencyInjection || State == GameModuleState.InitializeRules;

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
        /// The configuration of the module, setup at construction, used to pass runtime information
        /// </summary>
        public Configuration Configuration { get; private set; }

        internal bool IsService;
        internal GameProcess ParentProcess;

        internal RulesDictionary Rules;
        internal List<Type> InitUnloadOrder;
        internal List<RuleScheduling> UpdateScheduler;

        internal ExceptionPolicy ExceptionPolicy;
        internal PerformancePolicy PerformancePolicy;

        private QueueFSM<GameModuleState> m_StateMachine;
        private bool m_IsPaused;

        internal GameModule(IGameModuleSetup setup, Configuration configuration, GameProcess parentProcess)
        {
            IsService = setup is IGameServiceSetup;
            Name = string.Format("{0}{1}", setup.Name, IsService ? "Service" : "Mode");
            Configuration = configuration;
            ParentProcess = parentProcess;
            Rules = new RulesDictionary();
            if (IsService)
                Rules.AddRule(new ProcessAccessorRule(parentProcess));
            m_IsPaused = false;

            m_StateMachine = new QueueFSM<GameModuleState>($"{Name}FSM", new List<FSMState<GameModuleState>>()
            {
                new SetupState(this, setup),
                new DependencyInjectionState(this),
                new InitializeRulesState(this),
                new UpdateRulesState(this),
                new UnloadRulesState(this),
                new EndState()
            });
        }

        /// <summary>
        /// Pause the module. This will freeze the job and keep it in the same state until Restart
        /// </summary>
        public void Pause()
        {
            Log.Info(ParentProcess.Name, $"<< Pause {Name} >>");
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the module, which will undo the effects of Pause
        /// </summary>
        public void Restart()
        {
            Log.Info(ParentProcess.Name, $"<< Restart {Name} >>");
            m_IsPaused = false;
        }

        internal void Start()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameModuleState.Setup)
                throw new InvalidOperationException($"Start() should be called when job {Name} is in state Setup, not {State}");
#endif
            Log.Info(ParentProcess.Name, $"<< Load {Name} >>");
            m_StateMachine.Start();
        }

        internal void Update()
        {
            if (!m_IsPaused)
            {
                m_StateMachine.Update();
            }
        }

        internal void Unload()
        {
            if (State == GameModuleState.UnloadRules || State == GameModuleState.End)
                return;

            Log.Info(ParentProcess.Name, $"<< Unload {Name} >>");

            byte priority = 100;
            while (m_StateMachine.TryDequeueState(out GameModuleState state, priority: priority++) && state != GameModuleState.UnloadRules) ;

            if (State == GameModuleState.Setup || State == GameModuleState.DependencyInjection)
                m_StateMachine.DequeueState(priority: priority++);
        }

        internal void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameModuleState.End)
                throw new InvalidOperationException($"Stop() should be called when job {Name} is in state End, not {State}");
#endif
            Log.Info(ParentProcess.Name, $"<< End {Name} >>");
            m_StateMachine.Stop();
        }

        internal void OnQuit()
        {
            Log.Info(ParentProcess.Name, $"<< Quit {Name} >>");

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
            m_StateMachine.Stop();
        }

        internal void GoToNextState()
        {
            m_StateMachine.DequeueState();
        }

        internal void AskUnload()
        {
            if (IsService)
                ParentProcess.Stop();
            else
            {
                try
                {
                    ParentProcess.SwitchToGameMode(ExceptionPolicy.FallbackMode);
                }
                catch (Exception e)
                {
                    Log.Exception(Name, e);
                    ParentProcess.SwitchToGameMode(null);
                }
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
                    AskUnload();
                    return true;
                case OnExceptionBehaviour.PauseAll:
                    ParentProcess.Pause();
                    return true;
                case OnExceptionBehaviour.StopAll:
                    ParentProcess.Stop();
                    return true;
                default:
                    return false;
            }
        }
    }
}
