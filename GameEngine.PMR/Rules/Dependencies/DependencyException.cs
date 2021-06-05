﻿using System;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// Exception thrown when a required dependency is not found amoung providers, most likely because the rule or service implementing it is missing
    /// </summary>
    public class DependencyException : ApplicationException
    {
        /// <summary>
        /// The type of the dependency (Rule or Service)
        /// </summary>
        public DependencyType DependencyType;

        /// <summary>
        /// The type of the rule that requires the dependency (through one of its properties)
        /// </summary>
        public Type DependentRule;

        /// <summary>
        /// The type of the dependency interface, for which no implementation was found
        /// </summary>
        public Type MissingInterface;

        /// <summary>
        /// Constructor of the DependencyException
        /// </summary>
        /// <param name="dependencyType">Type of the dependency</param>
        /// <param name="dependentRule">Type of the rule that requires the dependency</param>
        /// <param name="missingInterface">Type of the interface defining the dependency contract</param>
        public DependencyException(DependencyType dependencyType, Type dependentRule, Type missingInterface) :
            base($"Dependency injection failed for rule {dependentRule.Name}. Cannot find required interface {missingInterface.Name} in {dependencyType} providers")
        {
            DependencyType = dependencyType;
            DependentRule = dependentRule;
            MissingInterface = missingInterface;
        }
    }
}
