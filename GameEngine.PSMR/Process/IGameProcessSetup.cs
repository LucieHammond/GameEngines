using GameEngine.PSMR.Modes;
using GameEngine.PSMR.Services;
using System.Collections.Generic;

namespace GameEngine.PSMR.Process
{
    /// <summary>
    /// A setup model in the form of an interface to be implemented for defining the characteristics of a GameProcess
    /// </summary>
    public interface IGameProcessSetup
    {
        /// <summary>
        /// The name of the GameProcess
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A ServiceSetup configuring the ServiceMode that will be loaded and run during the whole lifetime of the GameProcess
        /// </summary>
        /// <returns>An instance of a custom ServiceSetup</returns>
        IServiceSetup GetServiceSetup();

        /// <summary>
        /// An ordered list of GameModeSetups that are expected to be loaded and run one after another by the GameProcess
        /// </summary>
        /// <returns>A list of instantiated custom GameModeSetups</returns>
        List<IGameModeSetup> GetFirstGameModes();
    }
}
