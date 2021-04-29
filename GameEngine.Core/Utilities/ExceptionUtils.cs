using System;
using System.Collections;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for exception checking operations
    /// </summary>
    public static class ExceptionUtils
    {
        private const string NULL_PARAM_MESSAGE = "Parameter cannot be null";
        private const string NULL_ELEMENT_MESSAGE = "Collection element cannot be null";
        private const string INVALID_PARAM_MESSAGE = "Parameter does not meet the expected conditions";

        /// <summary>
        /// Ensure that all given parameters are not null
        /// </summary>
        /// <param name="parameters">The parameters to check</param>
        /// <exception cref="ArgumentNullException">Thrown when at least one of the parameters is null</exception>
        public static void CheckNonNull(params object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                    throw new ArgumentNullException($"{i}", NULL_PARAM_MESSAGE);
            }
        }

        /// <summary>
        /// Ensure that the given collection parameter is not null and that none of its element is null either
        /// </summary>
        /// <param name="parameter">The collection parameter to check</param>
        /// <param name="paramName">The name of the collection parameter</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection (or one of its elements) is null</exception>
        public static void CheckNonNullCollection(ICollection parameter, string paramName = null)
        {
            if (parameter == null)
                throw new ArgumentNullException(paramName, NULL_PARAM_MESSAGE);

            int index = 0;
            foreach (object element in parameter)
            {
                if (element == null)
                    throw new ArgumentNullException($"{paramName ?? "param"}[{index}]", NULL_ELEMENT_MESSAGE);
                index++;
            }
        }

        /// <summary>
        /// Ensure that the given parameter meets the given condition
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="parameter">The parameter to check</param>
        /// <param name="condition">The function that evaluates the condition to be met for the parameter</param>
        /// <param name="message">The custom message to display in case of failure</param>
        /// <exception cref="ArgumentException">Thrown when the parameter fails to meet the condition</exception>
        public static void CheckCondition<T>(T parameter, Func<T, bool> condition, string message = INVALID_PARAM_MESSAGE)
        {
            if (!condition(parameter))
                throw new ArgumentException(message);
        }
    }
}
