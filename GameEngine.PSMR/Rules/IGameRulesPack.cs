using System.Collections.Generic;

namespace GameEngine.PSMR.Rules
{
    /// <summary>
    /// Represent a group of rules that are linked to each other and therefore need to be added together in a process
    /// </summary>
    public interface IGameRulesPack
    {
        /// <summary>
        /// Create and return the group of linked game rules
        /// </summary>
        /// <returns>An enumerable structure containing all the rules</returns>
        IEnumerable<GameRule> GetRules();
    }
}
