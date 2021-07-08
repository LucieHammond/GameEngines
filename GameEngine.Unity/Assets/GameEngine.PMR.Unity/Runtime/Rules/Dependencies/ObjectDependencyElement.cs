namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// The different parts of a gameobject that can be referenced in a rule as a dependency 
    /// </summary>
    public enum ObjectDependencyElement
    {
        /// <summary>
        /// The gameobject itself
        /// </summary>
        GameObject,

        /// <summary>
        /// The transform of the gameobject
        /// </summary>
        Transform,

        /// <summary>
        /// A component of the gameobject
        /// </summary>
        Component
    }
}
