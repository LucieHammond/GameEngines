using GameEngine.PJR.Jobs.Policies;
using GameEngine.PJR.Rules;
using GameEngine.PJR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEngine.PJR.Jobs
{
    /// <summary>
    /// A setup model in the form of an interface to be implemented for defining the characteristics of a custom GameJob
    /// </summary>
    public interface IGameJobSetup
    {
        /// <summary>
        /// The name of the GameJob
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Instantiate all the GameRules of the GameJob and add them to the given RulesDictionary
        /// </summary>
        /// <param name="rules">The dictionary storing all the GameRules used in the GameJob</param>
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
        /// Define a set of configurations concerning error handling in the GameJob
        /// </summary>
        /// <returns>An ErrorPolicy object</returns>
        ErrorPolicy GetErrorPolicy();

        /// <summary>
        /// Define a set of configurations concerning performance management in the GameJob
        /// </summary>
        /// <returns>A PerformancePolicy object</returns>
        PerformancePolicy GetPerformancePolicy();
    }
}
