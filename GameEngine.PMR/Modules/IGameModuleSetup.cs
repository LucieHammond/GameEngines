using GameEngine.PMR.Modules.Policies;
using GameEngine.PMR.Modules.Specialization;
using GameEngine.PMR.Process.Transitions;
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
        /// The name of the module
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The service setup that the submodule requires (its rules can have service dependencies to it).
        /// Set null if no service setup is required
        /// </summary>
        Type RequiredServiceSetup { get; }

        /// <summary>
        /// The parent module setup that the submodule requires (its rules can have rule dependencies to it, and recursively to all the hierarchy).
        /// Set null if no parent module setup is required
        /// </summary>
        Type RequiredParentSetup { get; }

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
        /// Define and return a schedule to follow for organizing the update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        List<RuleScheduling> GetUpdateScheduler();

        /// <summary>
        /// Define and return a schedule to follow for organizing the fixed update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        List<RuleScheduling> GetFixedUpdateScheduler();

        /// <summary>
        /// Define and return a schedule to follow for organizing the late update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        List<RuleScheduling> GetLateUpdateScheduler();

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
        /// Define a number of additional specialized tasks to be performed before initialization and after unload
        /// </summary>
        /// <param name="tasks">The list of special tasks that will be performed by the module, to be completed</param>
        void SetSpecialTasks(ref List<SpecialTask> tasks);

        /// <summary>
        /// Define the transition activity that should be displayed when the module is loaded or unloaded
        /// </summary>
        /// <returns>A Transition object, or null if no transition should be used</returns>
        Transition GetTransition();
    }
}
