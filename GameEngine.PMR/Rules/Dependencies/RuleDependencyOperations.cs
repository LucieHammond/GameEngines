using GameEngine.PMR.Rules.Dependencies.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// A class providing methods for extracting or injecting rule dependencies with reflexion
    /// </summary>
    internal static class RuleDependencyOperations
    {
        internal static DependencyProvider ExtractDependencies(RulesDictionary rules)
        {
            DependencyProvider dependencyProvider = new DependencyProvider();
            foreach (KeyValuePair<Type, GameRule> ruleInfo in rules)
            {
                RuleAccessAttribute providerAtt = ruleInfo.Key.GetCustomAttribute<RuleAccessAttribute>();
                if (providerAtt != null)
                {
                    dependencyProvider.Add(providerAtt.AccessInterface, ruleInfo.Value);
                }
            }
            return dependencyProvider;
        }

        internal static void InjectRuleDependencies(this GameRule rule, DependencyProvider servicesProvider, DependencyProvider moduleProvider)
        {
            bool ruleProvider(RuleDependencyAttribute attribute, Type fieldType, out object result, out string error)
            {
                result = null;
                error = string.Empty;

                DependencyProvider dependencySource;
                bool inherited;
                switch (attribute.Source)
                {
                    case RuleDependencySource.SameModule:
                        dependencySource = moduleProvider;
                        inherited = false;
                        break;
                    case RuleDependencySource.RelatedModule:
                        dependencySource = moduleProvider;
                        inherited = true;
                        break;
                    case RuleDependencySource.Service:
                    default:
                        dependencySource = servicesProvider;
                        inherited = false;
                        break;
                }

                if (dependencySource == null)
                {
                    error = "No corresponding dependency provider was given";
                    return false;
                }
                else if (!dependencySource.TryGet(fieldType, out result, inherited))
                {
                    error = $"The corresponding dependency provider doesn't contain a reference to {fieldType}";
                    return false;
                }
                return true;
            };

            DependencyOperations.InjectDependencies<RuleDependencyAttribute>(rule, ruleProvider);
        }
    }
}
