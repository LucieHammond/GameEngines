using System;
using System.Collections.Generic;

namespace GameEngine.PSMR.Dependencies
{
    internal class DependencyProvider
    {
        private Dictionary<Type, object> m_Dependencies;

        internal DependencyProvider()
        {
            m_Dependencies = new Dictionary<Type, object>();
        }

        internal void Add(Type interfaceType, object dependency)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"Cannot add {interfaceType} as dependency because {interfaceType} is not an interface");

            if (!dependency.GetType().IsAssignableFrom(interfaceType))
                throw new ArgumentException($"The class {dependency.GetType()} does not implement the interface {interfaceType} that it is supposed to provide");

            if (m_Dependencies.ContainsKey(interfaceType))
                throw new InvalidOperationException($"Dependency provider already contains a reference for interface {interfaceType}");

            m_Dependencies.Add(interfaceType, dependency);
        }

        internal bool TryGet(Type interfaceType, out object dependency)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException($"Cannot inject dependency for type {interfaceType} because {interfaceType} is not an interface");

            return m_Dependencies.TryGetValue(interfaceType, out dependency);
        }
    }
}
