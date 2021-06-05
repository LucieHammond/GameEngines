using GameEngine.PMR.Modules;
using System;

namespace GameEngine.PMR.Process.Modes
{
    /// <summary>
    /// The setup interface to be implemented for defining GameModes (which are a certain kind of GameJobs, time-limited and service dependent)
    /// <seealso cref="IGameModuleSetup"/>
    /// </summary>
    public interface IGameModeSetup : IGameModuleSetup
    {
        /// <summary>
        /// The ServiceSetup that the GameMode requires (its rules can have dependencies to it)
        /// Set null if no ServiceSetup is required
        /// </summary>
        Type RequiredServiceSetup { get; }
    }
}
