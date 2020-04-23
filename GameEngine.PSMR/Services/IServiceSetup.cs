using GameEngine.PSMR.Modes;
using GameEngine.PSMR.Rules;
using System;
using System.Collections.Generic;

namespace GameEngine.PSMR.Services
{
    /// <summary>
    /// A setup model to be implemented for defining a custom ServiceMode, which is a kind of a GameMode handling GameServices instead of GameRules
    /// </summary>
    public interface IServiceSetup : IGameModeSetup
    {
        /// <summary>
        /// The ServiceSetup requirement makes no sense for a GameMode that is already a ServiceMode
        /// A ServiceSetup cannot depend on another ServiceSetup. So default value is null.
        /// </summary>
        new Type RequiredServiceSetup => null;

        /// <summary>
        /// Instantiate all the GameServices of the ServiceMode and add them to the given RulesDictionary
        /// </summary>
        /// <param name="rules">The dictionary storing all the GameServices</param>
        void SetServices(ref RulesDictionary rules);

        new void SetRules(ref RulesDictionary rules)
        {
            SetServices(ref rules);

            foreach (KeyValuePair<Type, GameRule> rule in rules)
            {
                if (!(rule.Value is GameService))
                {
                    throw new InvalidCastException($"Cannot setup rule {rule.Value.Name} in ServiceSetup because it doesn't inherit GameService");
                }
            }
        }
    }
}
