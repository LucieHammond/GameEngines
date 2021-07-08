using System;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// An attribute to use on GameRules in order to enable other rules to reference them via customized interfaces.
    /// When used as a dependency provider, a GameRule must implement the interface that the dependency consumers will use.
    /// All providers will be found and referenced using reflexion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RuleAccessAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the RuleAccessAttribute
        /// </summary>
        /// <param name="accessInterface">The type of the interface to implement</param>
        public RuleAccessAttribute(Type accessInterface)
        {
            AccessInterface = accessInterface;
        }

        /// <summary>
        /// The type of the interface that defines the contract on which the dependent rules can rely
        /// </summary>
        public Type AccessInterface;
    }
}
