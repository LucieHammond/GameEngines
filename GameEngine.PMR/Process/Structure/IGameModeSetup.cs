using GameEngine.PMR.Modules;

namespace GameEngine.PMR.Process.Structure
{
    /// <summary>
    /// The setup interface to be implemented for defining GameModes (which are a certain kind of modules)
    /// <seealso cref="IGameModuleSetup"/>
    /// </summary>
    public interface IGameModeSetup : IGameModuleSetup
    {
        /// <summary>
        /// The service setup that the game mode requires (its rules can have service dependencies to it).
        /// Set null if no service setup is required
        /// </summary>
        string RequiredServiceSetup { get; }
    }
}
