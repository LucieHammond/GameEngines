using System;

namespace GameEngine.PJR.Rules.Scheduling
{
    /// <summary>
    /// The association of a game rule type and a schedule pattern, used to plan events like updates for the rules of this type
    /// </summary>
    public class RuleScheduling
    {
        /// <summary>
        /// The rule type to associate with the schedule pattern
        /// </summary>
        public Type RuleType { get; private set; }

        /// <summary>
        /// The schedule pattern to associate with the rule type
        /// </summary>
        public SchedulePattern Pattern { get; private set; }

        /// <summary>
        /// Constructor of a RuleScheduling
        /// </summary>
        /// <param name="ruleType">The type of the rule</param>
        /// <param name="frequency">The number of frames to count between two scheduled events of the rule</param>
        /// <param name="offset">The position of the scheduled event relatively to each periodic cycle of frames</param>
        public RuleScheduling(Type ruleType, byte frequency, byte offset) : this(ruleType, new SchedulePattern(frequency, offset))
        {

        }

        /// <summary>
        /// Constructor of a RuleScheduling
        /// </summary>
        /// <param name="ruleType">The type of the rule</param>
        /// <param name="pattern">The schedule pattern to be associated with this type of rule</param>
        public RuleScheduling(Type ruleType, SchedulePattern pattern)
        {
            if (!ruleType.IsSubclassOf(typeof(GameRule)))
                throw new ArgumentException($"{ruleType} is not a type of GameRule", "ruleType");

            RuleType = ruleType;
            Pattern = pattern;
        }

        internal bool IsExpectedAtFrame(int frameCount)
        {
            return Pattern.IsFrameIncluded(frameCount);
        }
    }
}
