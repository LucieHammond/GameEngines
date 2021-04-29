using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.Core.Utilities
{
    /// <summary>
    /// An utility class regrouping useful methods for reflection operations
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Retrieve the type corresponding to the given name, performing a case-sensitive search in some chosen assemblies
        /// </summary>
        /// <param name="typeName">The full name of the type (qualified with namespace)</param>
        /// <param name="checkAllAssemblies">
        /// If false, check only the mscorlib assembly and the executing assembly;
        /// If true, check all assemblies loaded into the execution context of the application
        /// </param>
        /// <returns>The type corresponding to the name, if found</returns>
        public static Type GetTypeFromName(string typeName, bool checkAllAssemblies = true)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (checkAllAssemblies)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve all types that can be assigned to a given parent type, performing a search in a chosen assembly
        /// </summary>
        /// <param name="parentType">The parent type to search for (a class to inherit or an interface to implement)</param>
        /// <param name="assembly">The assembly to search in. If null, take the calling assembly</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetDerivedTypes(Type parentType, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            return assembly.GetTypes().Where((type) => parentType.IsAssignableFrom(type) && type != parentType).ToArray();
        }

        /// <summary>
        /// Retrieve all types that can be assigned to a given parent type, performing a search in a chosen list of assemblies
        /// </summary>
        /// <param name="parentType">The parent type to search for (a class to inherit or an interface to implement)</param>
        /// <param name="assemblies">The assemblies to search in</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetDerivedTypes(Type parentType, IEnumerable<Assembly> assemblies)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(GetDerivedTypes(parentType, assembly));
            }
            return types.ToArray();
        }

        /// <summary>
        /// Retrieve all types that have a given attribute applied to them, performing a search in a chosen assembly
        /// </summary>
        /// <param name="attributeType">The attribute type to search for</param>
        /// <param name="assembly">The assembly to search in. If null, take the calling assembly</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetTypesWithAttribute(Type attributeType, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            return assembly.GetTypes().Where((type) => type.GetCustomAttribute(attributeType, true) != null).ToArray();
        }

        /// <summary>
        /// Retrieve all types that have a given attribute applied to them, performing a search in a chosen list of assemblies
        /// </summary>
        /// <param name="attributeType">The attribute type to search for</param>
        /// <param name="assemblies">The assemblies to search in</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetTypesWithAttribute(Type attributeType, IEnumerable<Assembly> assemblies)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(GetTypesWithAttribute(attributeType, assembly));
            }
            return types.ToArray();
        }

        /// <summary>
        /// Retrieve all types that match a given condition, performing a search in a chosen assembly
        /// </summary>
        /// <param name="condition">The custom function that evaluates the condition to be met for a type</param>
        /// <param name="assembly">The assembly to search in. If null, take the calling assembly</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetTypesMatchingCondition(Func<Type, bool> condition, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            return assembly.GetTypes().Where((type) => condition(type)).ToArray();
        }

        /// <summary>
        /// Retrieve all types that match a given condition, performing a search in a chosen list of assemblies
        /// </summary>
        /// <param name="condition">The custom function that evaluates the condition to be met for a type</param>
        /// <param name="assemblies">The assemblies to search in</param>
        /// <returns>An array containing all the types found</returns>
        public static Type[] GetTypesMatchingCondition(Func<Type, bool> condition, IEnumerable<Assembly> assemblies)
        {
            List<Type> types = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                types.AddRange(GetTypesMatchingCondition(condition, assembly));
            }
            return types.ToArray();
        }
    }
}
