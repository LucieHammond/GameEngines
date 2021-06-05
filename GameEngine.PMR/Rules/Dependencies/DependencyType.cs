namespace GameEngine.PMR.Rules.Dependencies
{
    /// <summary>
    /// Possible types of dependency injections, corresponding to different kind of providers
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// The dependency provider is a GameRule that is part of the same GameModule or a parent GameModule
        /// </summary>
        Rule,

        /// <summary>
        /// The dependency provider is a GameRule that is registered as part of the Services module executed by the parent GameProcess
        /// </summary>
        Service
    }
}
