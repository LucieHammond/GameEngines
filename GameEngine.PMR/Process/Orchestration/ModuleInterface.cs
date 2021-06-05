using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Modules;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Process.Structure;
using System;

namespace GameEngine.PMR.Process.Orchestration
{
    /// <summary>
    /// A static class providing safe public interfaces for requiring changes on a GameModule (which will be handled by an orchestrator)
    /// </summary>
    public static class ModuleInterface
    {
        /// <summary>
        /// Unload the current module
        /// </summary>
        /// <param name="module">The module to unload</param>
        public static void Unload(this GameModule module)
        {
            module.Orchestrator.UnloadModule();
        }

        /// <summary>
        /// Reload the current module
        /// </summary>
        /// <param name="module">The module to reload</param>
        public static void Reload(this GameModule module)
        {
            module.Orchestrator.ReloadModule();
        }

        /// <summary>
        /// Switch to a different given module, i.e unload the current module and then load the new one instead
        /// </summary>
        /// <param name="module">The current module</param>
        /// <param name="setup">The setup defining the new module</param>
        /// <param name="configuration">The initial configuration of the new module. If not set, a pre-registered configuration will be used</param>
        public static void SwitchToModule(this GameModule module, IGameSubmoduleSetup setup, Configuration configuration = null)
        {
            module.Orchestrator.SwitchToModule(setup, configuration);
        }

        /// <summary>
        /// Load a given submodule as a child of the current module
        /// </summary>
        /// <param name="module">The parent module on which to load the submodule</param>
        /// <param name="setup">The setup defining the submodule to load</param>
        /// <param name="configuration">The initial configuration of the submodule. If not set, a pre-registered configuration will be used</param>
        public static void LoadSubmodule(this GameModule module, IGameSubmoduleSetup setup, Configuration configuration = null)
        {
            try
            {
                module.Orchestrator.AddSubmodule(setup, configuration);
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(ModuleOrchestrator.TAG, e);
            }
        }

        /// <summary>
        /// Unload the given child submodule from the current module
        /// </summary>
        /// <param name="module">The parent module that contains a reference to the submodule</param>
        /// <param name="submodule">The submodule to unload</param>
        public static void UnloadSubmodule(this GameModule module, GameModule submodule)
        {
            try
            {
                module.Orchestrator.RemoveSubmodule(submodule);
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(ModuleOrchestrator.TAG, e);
            }
        }

        /// <summary>
        /// Get the transition associated with the module for its loading and unloading phase
        /// </summary>
        /// <param name="module">The current module</param>
        /// <returns>The current transition activity, managed by the orchestrator</returns>
        public static TransitionActivity GetTransition(this GameModule module)
        {
            return module.Orchestrator.CurrentTransition;
        }
    }
}
