using GameEngine.PJR.Jobs;

namespace GameEngine.PJR.Process.Services
{
    /// <summary>
    /// The setup interface to be implemented for defining ServiceHandlers (which are a certain kind of GameJobs, unique and lasting throughout the GameProcess)
    /// <seealso cref="IGameJobSetup"/>
    /// </summary>
    public interface IServiceSetup : IGameJobSetup
    {

    }
}
