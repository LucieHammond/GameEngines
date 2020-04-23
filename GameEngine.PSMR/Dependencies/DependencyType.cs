﻿namespace GameEngine.PSMR.Dependencies
{
    /// <summary>
    /// Possible types of dependency injections, corresponding to different kind of providers
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// The dependency provider is a GameRule that is part of the same GameMode (not available on GameServices)
        /// </summary>
        Rule,

        /// <summary>
        /// The dependency provider is a GameService that is part of the ServiceMode executed by the parent game process
        /// </summary>
        Service,

        /// <summary>
        /// The injected value is a IConfiguration object containing context information passed to the current GameMode at creation
        /// </summary>
        Config
    }
}
