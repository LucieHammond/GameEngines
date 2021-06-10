using GameEngine.PMR.Modules;
using System;

namespace GameEngine.PMR.Process.Structure
{
    /// <summary>
    /// The setup interface to be implemented for defining GameSubmodules (which are a certain kind of modules)
    /// <seealso cref="IGameModuleSetup"/>
    /// </summary>
    public interface IGameSubmoduleSetup : IGameModuleSetup
    {
        /// <summary>
        /// The service setup that the submodule requires (its rules can have service dependencies to it).
        /// Set null if no service setup is required
        /// </summary>
        Type RequiredServiceSetup { get; }

        /// <summary>
        /// The parent module setup that the submodule requires (its rules can have rule dependencies to it, and recursively to all the hierarchy).
        /// Set null if no parent module setup is required
        /// </summary>
        Type RequiredParentSetup { get; }
    }
}
