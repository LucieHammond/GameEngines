using System.Collections.Generic;

namespace GameEngine.PMR.Rules
{
    /// <summary>
    /// An interface to be derived on all Unity rules that require loading scenes and referencing dependencies on game objects
    /// </summary>
    public interface ISceneGameRule
    {
        /// <summary>
        /// The scenes required by the rule
        /// </summary>
        HashSet<string> RequiredScenes { get; }
    }
}
