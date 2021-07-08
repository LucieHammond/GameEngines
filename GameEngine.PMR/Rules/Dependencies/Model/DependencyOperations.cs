using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.PMR.Rules.Dependencies.Model
{
    /// <summary>
    /// A delegate defining the format of a customizable dependency provider method used to search for a requested dependency
    /// </summary>
    /// <typeparam name="TAttribute">The type of dependency attribute characterizing the dependency to search</typeparam>
    /// <param name="dependencyAttribute">The dependency attribute characterizing the dependency to search</param>
    /// <param name="fieldType">The type of the field that is defined as a dependency</param>
    /// <param name="result">The result of the dependency search</param>
    /// <param name="error">A string representing an eventual error that occured</param>
    /// <returns>A boolean indicating if the dependency search was successful</returns>
    public delegate bool DependencyProviderDelegate<TAttribute>(TAttribute dependencyAttribute, Type fieldType, out object result, out string error);

    /// <summary>
    /// A class providing methods for injecting the dependencies declared in a list of rules with reflexion
    /// </summary>
    public static class DependencyOperations
    {
        /// <summary>
        /// Set the value of all dependencies declared in the rule with a specific type of DependencyAttribute, using a given provider method
        /// </summary>
        /// <typeparam name="TAttribute">The type of dependency attribute characterizing the dependencies to inject</typeparam>
        /// <param name="rule">The rule on which to inject dependencies</param>
        /// <param name="provider">The method to use for finding the relevant dependency values</param>
        public static void InjectDependencies<TAttribute>(this GameRule rule, DependencyProviderDelegate<TAttribute> provider)
            where TAttribute : DependencyAttribute
        {
            foreach (FieldInfo field in rule.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => field.IsDefined(typeof(TAttribute), true)))
            {
                TAttribute attribute = field.GetCustomAttribute<TAttribute>();

                if (provider(attribute, field.FieldType, out object dependency, out string error))
                {
                    field.SetValue(rule, dependency);
                }
                else if (attribute.Required)
                {
                    throw new DependencyException(attribute, field.FieldType, rule.GetType(), error);
                }
            }
        }
    }
}
