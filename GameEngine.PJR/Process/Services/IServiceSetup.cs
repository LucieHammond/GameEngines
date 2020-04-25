using GameEngine.PJR.Jobs;
using GameEngine.PJR.Rules;
using System;
using System.Collections.Generic;

namespace GameEngine.PJR.Process.Services
{
    /// <summary>
    /// The setup interface to be implemented for defining ServiceHandlers (which are a certain kind of GameJobs, unique and lasting throughout the GameProcess)
    /// <seealso cref="IGameJobSetup"/>
    /// </summary>
    public interface IServiceSetup : IGameJobSetup
    {
        internal void CheckOnlyServices(RulesDictionary rules)
        {
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
