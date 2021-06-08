using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Transitions;
using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Scheduling;
using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Modules
{
    /// <summary>
    /// A setup model in the form of an interface to be implemented for defining the characteristics of a custom GameModule
    /// </summary>
    public interface IGameModuleSetup
    {
        /// <summary>
        /// The name of the module, used as an identifier
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Instantiate all the rules of the module and add them to the given RulesDictionary
        /// </summary>
        /// <param name="rules">The dictionary storing all the GameRules used in the module</param>
        void SetRules(ref RulesDictionary rules);

        /// <summary>
        /// Set and return the order in which the rules must be initialized
        /// By deduction, the unloading operations will be done in reverse order
        /// </summary>
        /// <returns>An ordered list of the GameRule types</returns>
        List<Type> GetInitUnloadOrder();

        /// <summary>
        /// Set and return a schedule to follow for organizing the update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        List<RuleScheduling> GetUpdateScheduler();

        /// <summary>
        /// Define a set of configurations concerning exception handling in the module
        /// </summary>
        /// <returns>An ExceptionPolicy object</returns>
        ExceptionPolicy GetExceptionPolicy();

        /// <summary>
        /// Define a set of configurations concerning performance management in the module
        /// </summary>
        /// <returns>A PerformancePolicy object</returns>
        PerformancePolicy GetPerformancePolicy();

        /// <summary>
        /// Define the transition activity that should be displayed when the module is loaded or unloaded
        /// </summary>
        /// <returns>A TransitionActivity object, or null if no transition should be used</returns>
        TransitionActivity GetTransitionActivity();
    }
}
