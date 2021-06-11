using GameEngine.PMR.Modules;

namespace GameEngine.PMR.Process.Structure
{
    /// <summary>
    /// The setup interface to be implemented for defining GameServices (which are a certain kind of modules)
    /// <seealso cref="IGameModuleSetup"/>
    /// </summary>
    public interface IGameServiceSetup : IGameModuleSetup
    {
        /// <summary>
        /// Define custom controls on the application environment that must be checked for the services to work
        /// </summary>
        /// <returns>Indicate if the custom conditions are met</returns>
        bool CheckAppRequirements();
    }
}
