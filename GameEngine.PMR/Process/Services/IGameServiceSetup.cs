using GameEngine.PMR.Modules;

namespace GameEngine.PMR.Process.Services
{
    /// <summary>
    /// The setup interface to be implemented for defining GameServices (which are a certain kind of modules, unique and lasting throughout the GameProcess)
    /// <seealso cref="IGameModuleSetup"/>
    /// </summary>
    public interface IGameServiceSetup : IGameModuleSetup
    {

    }
}
