using System.Collections.Generic;

namespace GameEngine.Core.Logger
{
    /// <summary>
    /// A structure representing a target logger associated with log filtering policies
    /// </summary>
    public struct LogTarget
    {
        /// <summary>
        /// The logger to use for logging messages
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The minimal level of logs to display. All logs with an inferior level of importance will be ignored
        /// </summary>
        public LogLevel MinLevel { get; }

        /// <summary>
        /// The tags on which to filter the logs. If not null, only the logs having the right tags are displayed
        /// </summary>
        public HashSet<string> TagsFilter { get; }

        /// <summary>
        /// Initialize a new instance of LogTarget
        /// </summary>
        /// <param name="logger">The targetted logger to use</param>
        /// <param name="minLevel">The minimal level of logs to display with the targetted logger</param>
        /// <param name="tagsFilter">The tags filtering policy to apply with the targetted logger</param>
        public LogTarget(ILogger logger, LogLevel minLevel = LogLevel.Debug, HashSet<string> tagsFilter = null)
        {
            Logger = logger;
            MinLevel = minLevel;
            TagsFilter = tagsFilter;
        }
    }
}
