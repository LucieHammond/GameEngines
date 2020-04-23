using GameEngine.PSMR.Modes.Policies;
using GameEngine.PSMR.Rules;
using GameEngine.PSMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEngine.PSMR.Modes
{
    /// <summary>
    /// A setup model in the form of an interface to be implemented for defining the characteristics of a custom GameMode
    /// </summary>
    public interface IGameModeSetup
    {
        /// <summary>
        /// The name of the GameMode
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The ServiceSetup that the GameMode requires (its rules can have dependencies to it)
        /// Set null if no ServiceSetup is required
        /// </summary>
        Type RequiredServiceSetup { get; }

        /// <summary>
        /// Instantiate all the GameRules of the GameMode and add them to the given RulesDictionary
        /// </summary>
        /// <param name="rules">The dictionary storing all the GameRules used in the GameMode</param>
        void SetRules(ref RulesDictionary rules);

        /// <summary>
        /// Set and return the order in which the GameRules must be initialized
        /// By deduction, the unloading operations will be done in reverse order
        /// </summary>
        /// <returns>An ordered list of the GameRule types</returns>
        List<Type> GetInitUnloadOrder();

        /// <summary>
        /// Set and return a schedule to follow for organizing the update of the GameRules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        List<RuleScheduling> GetUpdateScheduler();

        /// <summary>
        /// Define a set of configurations concerning error handling in the GameMode
        /// </summary>
        /// <returns>An ErrorPolicy object</returns>
        ErrorPolicy GetErrorPolicy();

        /// <summary>
        /// Define a set of configurations concerning performance management in the GameMode
        /// </summary>
        /// <returns>A PerformancePolicy object</returns>
        PerformancePolicy GetPerformancePolicy();
    }
}
