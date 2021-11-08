namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// Possible types of sources for rule dependency injections, corresponding to different kind of rule providers
    /// </summary>
    public enum RuleDependencySource
    {
        /// <summary>
        /// The dependency is a rule that is part of the same module than the requesting rule
        /// </summary>
        SameModule,

        /// <summary>
        /// The dependency is a rule that is part of a module related to the one of the requesting rule (the same module or a parent)
        /// </summary>
        RelatedModule,

        /// <summary>
        /// The dependency is a rule that is registered as part of the Services module executed by the GameProcess
        /// </summary>
        Service
    }
}
