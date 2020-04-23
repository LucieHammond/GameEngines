﻿using System;

namespace GameEngine.PSMR.Dependencies.Attributes
{
    /// <summary>
    /// An attribute to use with game rule properties when you want to make them reference other rules or services via customized interfaces.
    /// The implementation of the interface to give to the property playing the role of a dependency consumer will be found among dependency providers.
    /// The value of the property will be injected using reflexion.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DependencyConsumerAttribute : Attribute
    {
        /// <summary>
        /// Constructor of the DependencyConsumerAttribute
        /// </summary>
        /// <param name="type">The type of the dependency</param>
        /// <param name="required">Whether the dependency is required</param>
        public DependencyConsumerAttribute(DependencyType type, bool required = true)
        {
            Type = type;
            Required = required;
        }

        /// <summary>
        /// Type of the dependency, specifying where to find the provider (among Services, Rules or Config)
        /// </summary>
        public DependencyType Type;

        /// <summary>
        /// If the dependency is required. A Required dependency will throw DependencyException if not found
        /// </summary>
        public bool Required = true;
    }
}
