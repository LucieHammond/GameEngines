using System;

namespace GameEngine.PMR.Rules.Dependencies.Model
{
    /// <summary>
    /// Exception thrown when a required dependency is not found amoung providers
    /// </summary>
    public class DependencyException : ApplicationException
    {
        private const string MESSAGE = "Failed to find the dependency to {0} required by the rule {1}.\n" +
            "Dependency information: {2}\n" +
            "Dependency error: {3}";

        /// <summary>
        /// The attribute characterizing the dependency
        /// </summary>
        public DependencyAttribute DependencyAttribute;

        /// <summary>
        /// Type of the rule property that is required as dependency
        /// </summary>
        public Type RequiredType;

        /// <summary>
        /// The type of the rule that requires the dependency (through one of its properties)
        /// </summary>
        public Type RequestingRule;

        /// <summary>
        /// A custom error message describing why the dependency was not found
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// Constructor of the DependencyException
        /// </summary>
        /// <param name="dependencyAttribute">The attribute characterizing the dependency</param>
        /// <param name="requiredType">Type of the rule property that is required as dependency</param>
        /// <param name="requestingRule">Type of the rule that requires the dependency</param>
        /// <param name="errorMessage">Custom error message describing why the dependency was not found</param>
        public DependencyException(DependencyAttribute dependencyAttribute, Type requiredType, Type requestingRule, string errorMessage) :
            base(string.Format(MESSAGE, requiredType, requestingRule, dependencyAttribute.ToString(), errorMessage))
        {
            DependencyAttribute = dependencyAttribute;
            RequiredType = requiredType;
            RequestingRule = requestingRule;
            ErrorMessage = errorMessage;
        }
    }
}
