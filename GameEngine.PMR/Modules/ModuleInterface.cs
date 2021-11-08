using GameEngine.Core.Logger;
using GameEngine.Core.System;
using GameEngine.PMR.Process.Orchestration;
using GameEngine.PMR.Process.Transitions;
using System;

namespace GameEngine.PMR.Modules
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
        public static void SwitchToModule(this GameModule module, IGameModuleSetup setup, Configuration configuration = null)
        {
            module.Orchestrator.SwitchToModule(setup, configuration);
        }

        /// <summary>
        /// Load a submodule as a child of the current module
        /// </summary>
        /// <param name="module">The parent module on which to load the submodule</param>
        /// <param name="subcategory">The name of the new subcategory to which the submodule (and its successors) will be attached</param>
        /// <param name="setup">The setup defining the submodule to load</param>
        /// <param name="configuration">The initial configuration of the submodule. If not set, a pre-registered configuration will be used</param>
        public static void LoadSubmodule(this GameModule module, string subcategory, IGameModuleSetup setup, Configuration configuration = null)
        {
            try
            {
                module.Orchestrator.AddSubmodule(subcategory, setup, configuration);
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(Orchestrator.TAG, e);
            }
        }

        /// <summary>
        /// Unload a child submodule from the current module
        /// </summary>
        /// <param name="module">The parent module that contains a reference to the submodule</param>
        /// <param name="subcategory">The subcategory to which the submodule is attached</param>
        public static void UnloadSubmodule(this GameModule module, string subcategory)
        {
            try
            {
                module.Orchestrator.RemoveSubmodule(subcategory);
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(Orchestrator.TAG, e);
            }
        }

        /// <summary>
        /// Retrieve one of the child submodule of the current module
        /// </summary>
        /// <param name="module">The parent module that contains a reference to the submodule</param>
        /// <param name="subcategory">The subcategory to which the submodule is attached</param>
        /// <returns>The retrieved submodule instance, or null if not found</returns>
        public static GameModule GetSubmodule(this GameModule module, string subcategory)
        {
            try
            {
                return module.Orchestrator.GetSubmodule(subcategory);
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(Orchestrator.TAG, e);
                return null;
            }
        }

        /// <summary>
        /// Get the transition associated with the module for its loading and unloading phase
        /// </summary>
        /// <param name="module">The current module</param>
        /// <returns>The current transition activity, managed by the orchestrator</returns>
        public static Transition GetTransition(this GameModule module)
        {
            return module.Orchestrator.CurrentTransition;
        }
    }
}
