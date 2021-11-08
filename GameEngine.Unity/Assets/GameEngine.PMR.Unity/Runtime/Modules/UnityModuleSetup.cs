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
    /// A model of module setup adapted to Unity (that includes Scenes composition and GameObject insertion)
    /// </summary>
    public abstract class UnityModuleSetup : IGameModuleSetup
    {
        /// <summary>
        /// The name of the module
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The service setup that the submodule requires (its rules can have service dependencies to it).
        /// Set null if no service setup is required
        /// </summary>
        public abstract Type RequiredServiceSetup { get; }

        /// <summary>
        /// The parent module setup that the submodule requires (its rules can have rule dependencies to it, and recursively to all the hierarchy).
        /// Set null if no parent module setup is required
        /// </summary>
        public abstract Type RequiredParentSetup { get; }

        /// <summary>
        /// Instantiate all the rules of the module and add them to the given RulesDictionary
        /// </summary>
        /// <param name="rules">The dictionary storing all the GameRules used in the module</param>
        public abstract void SetRules(ref RulesDictionary rules);

        /// <summary>
        /// Set and return the order in which the rules must be initialized
        /// By deduction, the unloading operations will be done in reverse order
        /// </summary>
        /// <returns>An ordered list of the GameRule types</returns>
        public abstract List<Type> GetInitUnloadOrder();

        /// <summary>
        /// Define and return a schedule to follow for organizing the update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        public abstract List<RuleScheduling> GetUpdateScheduler();

        /// <summary>
        /// Define and return a schedule to follow for organizing the fixed update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        public abstract List<RuleScheduling> GetFixedUpdateScheduler();

        /// <summary>
        /// Define and return a schedule to follow for organizing the late update of the rules
        /// </summary>
        /// <returns>A list of RuleScheduling objects, associating each rule type to a SchedulePattern</returns>
        public abstract List<RuleScheduling> GetLateUpdateScheduler();

        /// <summary>
        /// Define a set of configurations concerning exception handling in the module
        /// </summary>
        /// <returns>An ExceptionPolicy object</returns>
        public abstract ExceptionPolicy GetExceptionPolicy();

        /// <summary>
        /// Define a set of configurations concerning performance management in the module
        /// </summary>
        /// <returns>A PerformancePolicy object</returns>
        public abstract PerformancePolicy GetPerformancePolicy();

        /// <summary>
        /// Define a number of additional custom specialized tasks to be performed before initialization and after unload
        /// </summary>
        /// <returns>A list of SpecializedTask to be executed, possibly empty</returns>
        public List<SpecializedTask> GetSpecializedTasks()
        {
            return new List<SpecializedTask>()
            {
                new ScenesCompositionTask(GetDefaultScenes()),
                new GameObjectsInsertionTask()
            };
        }

        /// <summary>
        /// Define the transition activity that should be displayed when the module is loaded or unloaded
        /// </summary>
        /// <returns>A Transition object, or null if no transition should be used</returns>
        public abstract Transition GetTransition();

        /// <summary>
        /// Define a set of scenes that should be loaded with the module.
        /// The scenes can also be declared in the rules that need them
        /// </summary>
        /// <returns>A set of scenes characterized by their names</returns>
        public abstract HashSet<string> GetDefaultScenes();
    }
}
