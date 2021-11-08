using GameEngine.PMR.Rules.Dependencies.Model;
using System;

namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// An attribute to use on rule fields in order to make them reference gameobjects from loaded scenes.
    /// The value of the field will be injected using reflexion
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ObjectDependencyAttribute : DependencyAttribute
    {
        /// <summary>
        /// The type of the dependency
        /// <see cref="DependencyAttribute.DependencyType"/>
        /// </summary>
        public override string DependencyType => "GameObject Dependency";

        /// <summary>
        /// The name of the gameobject to reference
        /// </summary>
        public string ObjectName;

        /// <summary>
        /// The element of the gameobject to reference (the object itself, the transform or a component)
        /// </summary>
        public ObjectDependencyElement ObjectElement;

        /// <summary>
        /// Constructor of ObjectDependencyAttribute
        /// </summary>
        /// <param name="objectName">The name of the gameobject to reference</param>
        /// <param name="objectElement">The element of the gameobject to reference</param>
        /// <param name="required">Whether the dependency is required</param>
        public ObjectDependencyAttribute(string objectName, ObjectDependencyElement objectElement, bool required = true)
            : base(required)
        {
            ObjectName = objectName;
            ObjectElement = objectElement;
        }
    }
}
