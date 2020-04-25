using GameEngine.FSM;
using GameEngine.FSM.CustomFSM;
using GameEngine.PSMR.Modes.Policies;
using GameEngine.PSMR.Modes.States;
using GameEngine.PSMR.Process;
using GameEngine.PSMR.Rules;
using GameEngine.PSMR.Rules.Scheduling;
using GameEngine.PSMR.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace GameEngine.PSMR.Modes
{
    /// <summary>
    /// A subprocess called GameMode loading and running a coherent set of GameRules predefined in a GameModeSetup
    /// </summary>
    public class GameMode
    {
        /// <summary>
        /// The name of the GameMode, based on the name of the GameModeSetup
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state of the GameMode
        /// </summary>
        public GameModeState State => m_StateMachine.CurrentStateId;

        /// <summary>
        /// If the GameMode is currently in one of its initialization state
        /// </summary>
        public bool IsLoading => State == GameModeState.Setup || State == GameModeState.DependencyInjection || State == GameModeState.InitializeRules;

        /// <summary>
        /// If the GameMode is currently in its default operating state
        /// </summary>
        public bool IsOperational => State == GameModeState.UpdateRules;

        /// <summary>
        /// If the GameMode is currently in one of its unloading state
        /// </summary>
        public bool IsUnloading => State == GameModeState.UnloadRules;

        /// <summary>
        /// If the GameMode has finished its lifecycle and is waiting for closure
        /// </summary>
        public bool IsFinished => State == GameModeState.End;

        /// <summary>
        /// A floating number between 0 and 1 indicating the progression of the loading operations of the GameMode
        /// </summary>
        public float LoadingProgress { get; internal set; }

        internal bool IsServiceMode;
        internal IConfiguration InitialConfiguration;
        internal GameProcess ParentProcess;

        internal RulesDictionary Rules;
        internal List<Type> InitUnloadOrder;
        internal List<RuleScheduling> UpdateScheduler;

        internal ErrorPolicy ErrorPolicy;
        internal PerformancePolicy PerformancePolicy;

        private QueueFSM<GameModeState> m_StateMachine;
        private bool m_IsPaused;

        internal GameMode(IGameModeSetup setup, IConfiguration initialConfiguration, GameProcess parentProcess)
        {
            Name = $"{setup.Name}Mode";
            IsServiceMode = setup is IServiceSetup;
            InitialConfiguration = initialConfiguration;
            ParentProcess = parentProcess;
            Rules = new RulesDictionary();
            m_IsPaused = false;

            m_StateMachine = new QueueFSM<GameModeState>($"{Name}FSM", new List<FSMState<GameModeState>>()
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
        /// Pause the GameMode. This will freeze the mode and keep it in the same state until Restart
        /// </summary>
        public void Pause()
        {
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the GameMode, which will undo the effects of Pause
        /// </summary>
        public void Restart()
        {
            m_IsPaused = false;
        }

        internal void Start()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameModeState.Setup)
                throw new InvalidOperationException($"Start() should be called when mode {Name} is in state Setup, not {State}");
#endif
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
            if (State == GameModeState.UnloadRules || State == GameModeState.End)
                return;

            while (m_StateMachine.TryDequeueState(out GameModeState state, immediate: true) && state != GameModeState.UnloadRules) ;

            if (State == GameModeState.Setup || State == GameModeState.DependencyInjection)
                m_StateMachine.DequeueState(immediate: true);
        }

        internal void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameModeState.End)
                throw new InvalidOperationException($"Stop() should be called when mode {Name} is in state End, not {State}");
#endif
            m_StateMachine.Stop();
        }

        internal void OnQuit()
        {
            foreach (KeyValuePair<Type, GameRule> ruleInfo in Rules)
            {
                ruleInfo.Value.BaseQuit();
            }
        }

        internal void GoToNextState()
        {
            m_StateMachine.DequeueState();
        }

        internal bool OnError()
        {
            switch (ErrorPolicy.ReactionOnError)
            {
                case OnErrorBehaviour.Continue:
                    return false;
                case OnErrorBehaviour.SkipFrame:
                    return true;
                case OnErrorBehaviour.PauseMode:
                    Pause();
                    return true;
                case OnErrorBehaviour.UnloadMode:
                    try
                    {
                        ParentProcess.SwitchToGameMode(ErrorPolicy.FallbackMode);
                    }
                    catch (Exception)
                    {
                        ParentProcess.SwitchToGameMode(null);
                    }
                    return true;
                case OnErrorBehaviour.PauseAll:
                    ParentProcess.Pause();
                    return true;
                case OnErrorBehaviour.StopAll:
                    ParentProcess.Stop();
                    return true;
                default:
                    return false;
            }
        }
    }
}




