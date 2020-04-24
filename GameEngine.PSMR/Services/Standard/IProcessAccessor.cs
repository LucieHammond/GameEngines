using GameEngine.PSMR.Process;

namespace GameEngine.PSMR.Services.Standard
{
    /// <summary>
    /// A dependency interface used to allow the GameRules to access to the GameProcess that is responsible for them
    /// </summary>
    internal interface IProcessAccessor
    {
        GameProcess GetCurrentProcess();
    }
}
