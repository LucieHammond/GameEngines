namespace GameEngine.PMR.Process.Services
{
    /// <summary>
    /// A dependency interface used to allow the GameRules to access to the GameProcess that is responsible for them
    /// </summary>
    internal interface IProcessAccessor
    {
        GameProcess GetCurrentProcess();
    }
}
