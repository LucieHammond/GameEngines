using System;

namespace GameEngine.PJR.Rules.Dependencies.Attributes
{
    /// <summary>
    /// An attribute to use on GameRule properties in order to make them reference other rules or services via customized interfaces
    /// When used as a dependency consumer, a property must have an interface type so that its value can be found among dependency providers
    /// The value of the property will be injected using reflexion 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DependencyConsumerAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the DependencyConsumerAttribute
        /// </summary>
        /// <param name="type">The type of the dependency</param>
        /// <param name="required">Whether the dependency is required</param>
        public DependencyConsumerAttribute(DependencyType type, bool required = true)
        {
            Type = type;
            Required = required;
        }

        /// <summary>
        /// Type of the dependency, specifying where to find the provider (among Services, Rules or Config)
        /// </summary>
        public DependencyType Type;

        /// <summary>
        /// If the dependency is required. A Required dependency will throw DependencyException if not found
        /// </summary>
        public bool Required = true;
    }
}
