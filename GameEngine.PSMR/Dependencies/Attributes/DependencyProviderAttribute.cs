using System;

namespace GameEngine.PSMR.Dependencies.Attributes
{
    /// <summary>
    /// An attribute to use on GameRules or GameServices in order to enable other rules to reference them via customized interfaces
    /// When used as a dependency provider, a GameRule must implement the interface that the dependency consumers will use
    /// All providers will be found and referenced using reflexion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependencyProviderAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the DependencyProviderAttribute
        /// </summary>
        /// <param name="providedInterface">The type of the interface to implement</param>
        public DependencyProviderAttribute(Type providedInterface)
        {
            ProvidedInterface = providedInterface;
        }

        /// <summary>
        /// The type of the interface that defines the contract between the dependency provider and its consumers
        /// </summary>
        public Type ProvidedInterface;
    }
}
