namespace GameEngine.PMR.Modules.Policies
{
    /// <summary>
    /// Set of module configurations concerning performance measurement and optimization
    /// </summary>
    public class PerformancePolicy
    {
        /// <summary>
        /// The maximum time in ms a sequence of loading or unloading operations can take in one frame
        /// </summary>
        public int MaxFrameDuration;

        /// <summary>
        /// If the process should check for stalling rules and record a timeout when a threshold is exceeded
        /// </summary>
        public bool CheckStallingRules;

        /// <summary>
        /// Nb of times the init and unload process should send a stalling warning before throwing a TimeoutException
        /// </summary>
        public int NbWarningsBeforeException;

        /// <summary>
        /// The maximum time in ms a single rule can take to initialize before recording a timeout
        /// </summary>
        public int InitStallingTimeout;

        /// <summary>
        /// The maximum time in ms a single rule can take to update before recording a timeout
        /// </summary>
        public int UpdateStallingTimeout;

        /// <summary>
        /// The maximum time in ms a single rule can take to unload before recording a timeout
        /// </summary>
        public int UnloadStallingTimeout;
    }
}
