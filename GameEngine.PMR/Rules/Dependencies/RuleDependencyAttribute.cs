using GameEngine.PMR.Rules.Dependencies.Model;
using System;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// An attribute to use on rule fields in order to make them reference other rules via customized interfaces.
    /// When used as a dependency consumer, a field must have an interface type so that its value can be found among dependency providers.
    /// The value of the field will be injected using reflexion 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class RuleDependencyAttribute : DependencyAttribute
    {
        /// <summary>
        /// The type of the dependency
        /// <see cref="DependencyAttribute.DependencyType"/>
        /// </summary>
        public override string DependencyType => "Rule Dependency";

        /// <summary>
        /// The source of the dependency, specifying where to find the rule provider (services, current module, parent module)
        /// </summary>
        public RuleDependencySource Source;

        /// <summary>
        /// The constructor of RuleDependencyAttribute
        /// </summary>
        /// <param name="source">The source type of the dependency</param>
        /// <param name="required">Whether the dependency is required</param>
        public RuleDependencyAttribute(RuleDependencySource source, bool required = true) : base(required)
        {
            Source = source;
        }

        /// <summary>
        /// Returns a string that represents the current dependency attribute information
        /// </summary>
        /// <returns>A string that represents the current dependency attribute information</returns>
        public override string ToString()
        {
            return $"{base.ToString()}; Rule Source = {Source}";
        }
    }
}
