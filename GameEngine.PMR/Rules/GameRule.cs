using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using System;

namespace GameEngine.PMR.Rules
{
    /// <summary>
    /// Abstract template representing a GameRule. Each custom rule in a project must inherit from this class (or from a derivative)
    /// </summary>
    public abstract class GameRule
    {
        private const string TAG = "Rules";

        /// <summary>
        /// Name of the rule, which correspond to its type formatted as string
        /// </summary>
        public string Name => GetType().Name;

        /// <summary>
        /// State of the rule. Gives information about the lifecycle steps that the rule has passed
        /// </summary>
        public GameRuleState State { get; private set; }

        /// <summary>
        /// Warn that something went wrong during the rule lifecycle (when set to true)
        /// </summary>
        public bool ErrorDetected { get; private set; }

        /// <summary>
        /// A reference to the Game Process that is responsible for running that rule
        /// </summary>
        protected GameProcess m_Process;

        /// <summary>
        /// A reference to the Game Module to which the rule belongs
        /// </summary>
        protected GameModule m_Module;

        /// <summary>
        /// An object giving time information about the process pace (delta time, frame count, time since startup ...)
        /// </summary>
        protected ITime m_Time => m_Process.Time;

        /// <summary>
        /// Default constructor of a GameRule
        /// </summary>
        protected GameRule()
        {
            State = GameRuleState.Unused;
            ErrorDetected = false;
        }

        internal void InjectProcessDependencies(GameProcess process, GameModule module)
        {
            m_Process = process;
            m_Module = module;
        }

        internal void BaseInitialize()
        {
            Log.Info(TAG, $"Initializing rule {Name}");
            State = GameRuleState.Initializing;
            Initialize();
        }

        internal void BaseUpdate()
        {
            Update();
        }

        internal void BaseUnload()
        {
            Log.Info(TAG, $"Unloading rule {Name}");
            State = GameRuleState.Unloading;
            Unload();
        }

        internal void BaseQuit()
        {
            Log.Info(TAG, $"Quit rule {Name}");
            OnQuit();
        }

        /// <summary>
        /// The place where to define custom initializing operations for the rule. Will be called only once before any other method
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// The place where to define custom update operations for the rule. Will be called every relevant frame between Initialize() and Unload()
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// The place where to define custom unloading operations for the rule. Will be called only once after all other methods
        /// </summary>
        protected abstract void Unload();

        /// <summary>
        /// The place where to define quick and synchronous termination operations for the rule. Will be called when the application is abruptly closed
        /// </summary>
        protected virtual void OnQuit()
        {
            if (State == GameRuleState.Initialized)
                Unload();
        }

        /// <summary>
        /// Call this method to attest that the rule has correctly been initialized. This mecanism allows asynchronous initialization
        /// </summary>
        protected void MarkInitialized()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Initializing)
                throw new InvalidOperationException($"Invalid time context for calling MarkInitialized(). Current state: {State}. Expected state: Initializing");
#endif
            Log.Debug(TAG, $"Successfully complete the loading of rule {Name}");
            State = GameRuleState.Initialized;
        }

        /// <summary>
        /// Call this method to attest that the rule has correctly been unloaded. This mecanism allows asynchronous unloading
        /// </summary>
        protected void MarkUnloaded()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Unloading)
                throw new InvalidOperationException($"Invalid time context for calling MarkUnloaded(). Current state: {State}. Expected state: Unloading");
#endif
            Log.Debug(TAG, $"Successfully complete the unloading of rule {Name}");
            State = GameRuleState.Unloaded;
        }

        /// <summary>
        /// Call this method to alert that a blocking error occured during the rule's operations
        /// </summary>
        protected void MarkError()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State == GameRuleState.Unused || State == GameRuleState.Unloaded)
                throw new InvalidOperationException($"Invalid time context for calling MarkError() ({State}). Error may not be taken into account");
#endif
            Log.Debug(TAG, $"A blocking error has been detected in rule {Name}");
            State = GameRuleState.Unloaded;
            ErrorDetected = true;
        }
    }
}
