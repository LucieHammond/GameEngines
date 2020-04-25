using GameEngine.PJR.Jobs;
using System;

namespace GameEngine.PJR.Process.Modes
{
    /// <summary>
    /// The setup interface to be implemented for defining GameModes (which are a certain kind of GameJobs, time-limited and service dependent)
    /// <seealso cref="IGameJobSetup"/>
    /// </summary>
    public interface IGameModeSetup : IGameJobSetup
    {
        /// <summary>
        /// The ServiceSetup that the GameMode requires (its rules can have dependencies to it)
        /// Set null if no ServiceSetup is required
        /// </summary>
        Type RequiredServiceSetup { get; }
    }
}
