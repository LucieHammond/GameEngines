using System;

namespace GameEngine.PMR.Rules.Dependencies.Model
{
    /// <summary>
    /// An attribute to use on a rule's fields in order to reference dependencies that will be resolved before initialization using reflexion
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class DependencyAttribute : Attribute
    {
        /// <summary>
        /// The type of the dependency, characterizing which kind of objects are defined as dependency
        /// </summary>
        public abstract string DependencyType { get; }

        /// <summary>
        /// If the dependency is required. A Required dependency will throw a DependencyException if not found
        /// </summary>
        public bool Required = true;

        /// <summary>
        /// Constructor of the DependencyAttribute
        /// </summary>
        /// <param name="required">Whether the dependency is required</param>
        public DependencyAttribute(bool required = true)
        {
            Required = required;
        }

        /// <summary>
        /// Returns a string that represents the current dependency attribute information
        /// </summary>
        /// <returns>A string that represents the current dependency attribute information</returns>
        public override string ToString()
        {
            return $"Type = {DependencyType}; Required = {Required}";
        }
    }
}
