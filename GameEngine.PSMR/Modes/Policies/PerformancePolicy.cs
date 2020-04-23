namespace GameEngine.PSMR.Modes.Policies
{
    /// <summary>
    /// Set of GameMode configurations concerning performance measurement and optimization
    /// </summary>
    public class PerformancePolicy
    {
        /// <summary>
        /// The maximum time in ms a sequence of loading or unloading operations can take in one frame
        /// </summary>
        public int MaxFrameDuration;

        /// <summary>
        /// If the process should check for stalling rules and throw a TimeoutException when a threshold is exceeded
        /// </summary>
        public bool CheckStallingRules;

        /// <summary>
        /// The maximum time in ms a single rule can take to initialize before throwing a TimeoutException
        /// </summary>
        public int InitStallingTimeout;

        /// <summary>
        /// The maximum time in ms a single rule can take to update before throwing a TimeoutException
        /// </summary>
        public int UpdateStallingTimeout;

        /// <summary>
        /// The maximum time in ms a single rule can take to unload before throwing a TimeoutException
        /// </summary>
        public int UnloadStallingTimeout;
    }
}
