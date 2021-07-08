using System;
using System.Collections.Generic;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// A structure for storing available dependency interfaces and their associated implementations
    /// </summary>
    internal class DependencyProvider
    {
        private Dictionary<Type, object> m_Dependencies;
        private DependencyProvider m_ParentProvider;

        internal DependencyProvider()
        {
            m_Dependencies = new Dictionary<Type, object>();
        }

        internal void LinkToParentProvider(DependencyProvider parentProvider)
        {
            m_ParentProvider = parentProvider;
        }

        internal void Add(Type interfaceType, object dependency)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"Cannot add {interfaceType.Name} as dependency because {interfaceType.Name} is not an interface");

            if (!interfaceType.IsAssignableFrom(dependency.GetType()))
                throw new ArgumentException($"The class {dependency.GetType().Name} does not implement the interface {interfaceType.Name} that it is supposed to provide");

            if (m_Dependencies.ContainsKey(interfaceType))
                throw new InvalidOperationException($"Dependency provider already contains a reference for interface {interfaceType.Name}");

            m_Dependencies.Add(interfaceType, dependency);
        }

        internal bool TryGet(Type interfaceType, out object dependency, bool inherited = true)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"Cannot inject dependency for type {interfaceType.Name} because {interfaceType.Name} is not an interface");

            bool test = m_Dependencies.TryGetValue(interfaceType, out dependency);
            return m_Dependencies.TryGetValue(interfaceType, out dependency)
                || (inherited && m_ParentProvider != null && m_ParentProvider.TryGet(interfaceType, out dependency));
        }
    }
}
