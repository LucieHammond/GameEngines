using GameEngine.PSMR.Rules;

namespace GameEngine.PSMR.Services
{
    /// <summary>
    /// Abstract template representing a Game Service. Each custom service in a project must inherit from this class (or from a derivative)
    /// In fact a GameService is just a GameRule, the difference is essentially motivated by organizational needs
    /// </summary>
    public abstract class GameService : GameRule
    {
    }
}
