using GameEngine.FSM;
using GameEngine.FSM.CustomFSM;
using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Jobs.States;
using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Services;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Scheduling;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace GameEngine.PJR.Jobs
{
    /// <summary>
    /// A subprocess called GameJob loading and running a coherent set of GameRules predefined in a GameJobSetup
    /// </summary>
    public class GameJob
    {
        /// <summary>
        /// The name of the GameJob, based on the name of the GameJobSetup
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state of the GameJob
        /// </summary>
        public GameJobState State => m_StateMachine.CurrentStateId;

        /// <summary>
        /// If the GameJob is currently in one of its initialization state
        /// </summary>
        public bool IsLoading => State == GameJobState.Setup || State == GameJobState.DependencyInjection || State == GameJobState.InitializeRules;

        /// <summary>
        /// If the GameJob is currently in its default operating state
        /// </summary>
        public bool IsOperational => State == GameJobState.UpdateRules;

        /// <summary>
        /// If the GameJob is currently in one of its unloading state
        /// </summary>
        public bool IsUnloading => State == GameJobState.UnloadRules;

        /// <summary>
        /// If the GameJob has finished its lifecycle and is waiting for closure
        /// </summary>
        public bool IsFinished => State == GameJobState.End;

        /// <summary>
        /// A floating number between 0 and 1 indicating the progression of the loading operations of the GameJob
        /// </summary>
        public float LoadingProgress { get; internal set; }

        internal bool IsServiceJob;
        internal IConfiguration InitialConfiguration;
        internal GameProcess ParentProcess;

        internal RulesDictionary Rules;
        internal List<Type> InitUnloadOrder;
        internal List<RuleScheduling> UpdateScheduler;

        internal ErrorPolicy ErrorPolicy;
        internal PerformancePolicy PerformancePolicy;

        private QueueFSM<GameJobState> m_StateMachine;
        private bool m_IsPaused;

        internal GameJob(IGameJobSetup setup, IConfiguration initialConfiguration, GameProcess parentProcess)
        {
            Name = string.Format("{0}{1}", setup.Name, IsServiceJob ? "Services" : "Mode");
            IsServiceJob = setup is IServiceSetup;
            InitialConfiguration = initialConfiguration;
            ParentProcess = parentProcess;
            Rules = new RulesDictionary();
            m_IsPaused = false;

            m_StateMachine = new QueueFSM<GameJobState>($"{Name}FSM", new List<FSMState<GameJobState>>()
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
        /// Pause the GameJob. This will freeze the job and keep it in the same state until Restart
        /// </summary>
        public void Pause()
        {
            m_IsPaused = true;
        }

        /// <summary>
        /// Restart the GameJob, which will undo the effects of Pause
        /// </summary>
        public void Restart()
        {
            m_IsPaused = false;
        }

        internal void Start()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameJobState.Setup)
                throw new InvalidOperationException($"Start() should be called when job {Name} is in state Setup, not {State}");
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
            if (State == GameJobState.UnloadRules || State == GameJobState.End)
                return;

            while (m_StateMachine.TryDequeueState(out GameJobState state, immediate: true) && state != GameJobState.UnloadRules) ;

            if (State == GameJobState.Setup || State == GameJobState.DependencyInjection)
                m_StateMachine.DequeueState(immediate: true);
        }

        internal void Stop()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameJobState.End)
                throw new InvalidOperationException($"Stop() should be called when job {Name} is in state End, not {State}");
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
                case OnErrorBehaviour.PauseJob:
                    Pause();
                    return true;
                case OnErrorBehaviour.UnloadJob:
                    if (IsServiceJob)
                        ParentProcess.Stop();
                    else
                    {
                        try
                        {
                            ParentProcess.SwitchToGameMode(ErrorPolicy.FallbackMode);
                        }
                        catch (Exception)
                        {
                            ParentProcess.SwitchToGameMode(null);
                        }
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




