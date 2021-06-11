using GameEngine.PMR.Process.Structure;
using System.Collections.Generic;

namespace GameEngine.PMR.Process
{
    /// <summary>
    /// A setup model in the form of an interface to be implemented for defining the characteristics of a GameProcess
    /// </summary>
    public interface IGameProcessSetup
    {
        /// <summary>
        /// The name of the process
        /// </summary>
        string Name { get; }

        /// <summary>
        /// An IGameServiceSetup configuring the game service that will be loaded and run during the whole lifetime of the process
        /// </summary>
        /// <returns>An instance of a custom IGameServiceSetup</returns>
        IGameServiceSetup GetServiceSetup();

        /// <summary>
        /// An ordered list of IGameModeSetups configuring the game modes that are expected to be loaded and run one after another by the process
        /// </summary>
        /// <returns>A list of custom instances of IGameModeSetup</returns>
        List<IGameModeSetup> GetFirstGameModes();
    }
}
