﻿using System;

namespace GameEngine.PSMR.Rules
{
    /// <summary>
    /// Abstract template representing a Game Rule. Each custom rule in a project must inherit from this class (or from a derivative)
    /// </summary>
    public abstract class GameRule
    {
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
        /// Default constructor of a Game Rule
        /// </summary>
        protected GameRule()
        {
            State = GameRuleState.Unused;
            ErrorDetected = false;
        }

        /// <summary>
        /// The place where to define custom initialization operations for the rule. Will be called only once before any other method
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// The place where to define custom update operations for the rule. Will be called every frame between Initialize() and Unload()
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// The place where to define custom unload operations for the rule. Will be called only once after all other methods
        /// </summary>
        protected abstract void Unload();

        /// <summary>
        /// Call this method to attest that the rule has correctly been initialized. This mecanism allows asynchronous initialization
        /// </summary>
        protected void MarkInitialized()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Initializing)
                throw new InvalidOperationException($"MarkInitialized() should be called when rule {Name} is in state Initializing, not {State}");
#endif
            State = GameRuleState.Initialized;
        }

        /// <summary>
        /// Call this method to attest that the rule has correctly been unloaded. This mecanism allows asynchronous unload
        /// </summary>
        protected void MarkUnloaded()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Unloading)
                throw new InvalidOperationException($"MarkUnloaded() should be called when rule {Name} is in state Unloading, not {State}");
#endif
            State = GameRuleState.Unloaded;
        }

        /// <summary>
        /// Call this method to alert that a blocking error occured during the rule's operations
        /// </summary>
        protected void MarkError()
        {
            if (State == GameRuleState.Initializing)
                State = GameRuleState.Unloaded;
            ErrorDetected = true;
        }

        internal void BaseInitialize()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Unused)
                throw new InvalidOperationException($"Initialize() should be called when rule {Name} is in state Unused, not {State}");
#endif
            State = GameRuleState.Initializing;
            Initialize();
        }

        internal void BaseUpdate()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Initialized)
                throw new InvalidOperationException($"Update() should be called when rule {Name} is in state Initialized, not {State}");
#endif
            Update();
        }

        internal void BaseUnload()
        {
#if CHECK_OPERATIONS_CONTEXT
            if (State != GameRuleState.Initialized)
                throw new InvalidOperationException($"Unload() should be called when rule {Name} is in state Initialized, not {State}");
#endif
            State = GameRuleState.Unloading;
            Unload();
        }

        internal void OnQuit()
        {
            Unload();
        }
    }
}