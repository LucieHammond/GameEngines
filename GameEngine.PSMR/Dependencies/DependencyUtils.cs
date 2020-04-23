using GameEngine.PSMR.Dependencies.Attributes;
using GameEngine.PSMR.Rules;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.PSMR.Dependencies
{
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

        internal static void InjectDependencies(RulesDictionary rules, DependencyProvider servicesProvider, DependencyProvider rulesProvider, IConfiguration configuration)
        {
            foreach (KeyValuePair<Type, GameRule> ruleInfo in rules)
            {
                foreach (FieldInfo field in ruleInfo.Key.GetFields().Where(field => field.IsDefined(typeof(DependencyConsumerAttribute), false)))
                {
                    DependencyConsumerAttribute consumerAtt = field.GetCustomAttribute<DependencyConsumerAttribute>();

                    switch (consumerAtt.Type)
                    {
                        case DependencyType.Service:
                            if (servicesProvider != null && servicesProvider.TryGet(field.FieldType, out object service))
                            {
                                field.SetValue(ruleInfo.Value, service);
                                continue;
                            }
                            break;
                        case DependencyType.Rule:
                            if (rulesProvider != null && rulesProvider.TryGet(field.FieldType, out object rule))
                            {
                                field.SetValue(ruleInfo.Value, rule);
                                continue;
                            }
                            break;
                        case DependencyType.Config:
                            if (!field.FieldType.IsAssignableFrom(typeof(IConfiguration)))
                                throw new InvalidCastException($"Cannot use config dependency with a field of type {field.FieldType}. The type should be IConfiguration");

                            if (configuration != null)
                            {
                                field.SetValue(ruleInfo.Value, configuration);
                                continue;
                            }
                            break;
                    }

                    if (consumerAtt.Required)
                    {
                        throw new DependencyException(DependencyType.Service, ruleInfo.Key, field.FieldType);
                    }
                }
            }
        }
    }
}
