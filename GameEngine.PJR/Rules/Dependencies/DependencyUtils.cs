using GameEngine.PJR.Rules.Dependencies.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.PJR.Rules.Dependencies
{
    /// <summary>
    /// A class providing utils methods for exctracting or injecting dependencies with reflexion
    /// </summary>
    internal static class DependencyUtils
    {
        internal static DependencyProvider ExtractDependencies(RulesDictionary rules)
        {
            DependencyProvider dependencyProvider = new DependencyProvider();
            foreach (KeyValuePair<Type, GameRule> ruleInfo in rules)
            {
                DependencyProviderAttribute providerAtt = ruleInfo.Key.GetCustomAttribute<DependencyProviderAttribute>();
                if (providerAtt != null)
                {
                    dependencyProvider.Add(providerAtt.ProvidedInterface, ruleInfo.Value);
                }
            }
            return dependencyProvider;
        }

        internal static void InjectDependencies(RulesDictionary rules, DependencyProvider servicesProvider, DependencyProvider rulesProvider)
        {
            foreach (KeyValuePair<Type, GameRule> ruleInfo in rules)
            {
                foreach (FieldInfo field in ruleInfo.Key.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(field => field.IsDefined(typeof(DependencyConsumerAttribute), true)))
                {
                    DependencyConsumerAttribute consumerAtt = field.GetCustomAttribute<DependencyConsumerAttribute>();

                    DependencyProvider provider;
                    switch (consumerAtt.Type)
                    {
                        case DependencyType.Service:
                            provider = servicesProvider;
                            break;
                        case DependencyType.Rule:
                        default:
                            provider = rulesProvider;
                            break;
                    }

                    if (provider != null && provider.TryGet(field.FieldType, out object dependency))
                    {
                        field.SetValue(ruleInfo.Value, dependency);
                    }
                    else if (consumerAtt.Required)
                    {
                        throw new DependencyException(DependencyType.Service, ruleInfo.Key, field.FieldType);
                    }
                }
            }
        }
    }
}
