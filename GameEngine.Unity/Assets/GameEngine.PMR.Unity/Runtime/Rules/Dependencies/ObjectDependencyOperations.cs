using GameEngine.PMR.Rules;
using GameEngine.PMR.Rules.Dependencies.Model;
using System;
using UnityEngine;

namespace GameEngine.PMR.Unity.Rules.Dependencies
{
    /// <summary>
    /// A class providing methods for injecting gameobject dependencies with reflexion
    /// </summary>
    internal static class ObjectDependencyOperations
    {
        internal static void InjectObjectDependencies(this GameRule rule)
        {
            bool objectProvider(ObjectDependencyAttribute attribute, Type fieldType, out object result, out string error)
            {
                result = null;
                error = string.Empty;

                GameObject gameObject = GameObject.Find(attribute.ObjectName);
                if (gameObject == null)
                {
                    error = $"No gameobject named {attribute.ObjectName} was found in the scene hierarchy";
                    return false;
                }

                switch (attribute.ObjectElement)
                {
                    case ObjectDependencyElement.GameObject:
                        if (!fieldType.IsAssignableFrom(typeof(GameObject)))
                        {
                            error = $"Cannot affect a dependency value of type GameObject to a field of type {fieldType}";
                            return false;
                        }
                        result = gameObject;
                        break;
                    case ObjectDependencyElement.Transform:
                        if (!fieldType.IsAssignableFrom(typeof(Transform)))
                        {
                            error = $"Cannot affect a dependency value of type Transform to a field of type {fieldType}";
                            return false;
                        }
                        result = gameObject.transform;
                        break;
                    case ObjectDependencyElement.Component:
                        result = gameObject.GetComponent(fieldType);
                        if (result == null)
                        {
                            error = $"No component of type {fieldType} was found on the gameobject {attribute.ObjectName}";
                            return false;
                        }
                        break;
                }

                return result != null;
            }

            DependencyOperations.InjectDependencies<ObjectDependencyAttribute>(rule, objectProvider);
        }
    }
}
