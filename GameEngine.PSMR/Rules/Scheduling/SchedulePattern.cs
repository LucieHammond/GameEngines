namespace GameEngine.PSMR.Rules.Scheduling
{
    /// <summary>
    /// Logical pattern information describing how to schedule events like rule updates on specific frames
    /// </summary>
    public class SchedulePattern
    {
        /// <summary>
        /// Number of frames to count between two scheduled events.
        /// For example, 1 = update every frame, 2 = update one frame out of two, 0 = never update
        /// </summary>
        public byte Frequency { get; private set; }

        /// <summary>
        /// Position of the scheduled event relatively to each periodic cycle of frames.
        /// For example, considering cycles of 2 frames, 0 = update on the first frame in each cycle, 1 = update on the second frame in each cycle
        /// </summary>
        public byte Offset { get; private set; }

        /// <summary>
        /// Constructor of a SchedulePattern. By default, the pattern plan events for every frame
        /// </summary>
        /// <param name="frequency">Number of frames to count between two scheduled events</param>
        /// <param name="offset">Position of the scheduled event relatively to each periodic cycle of frames</param>
        public SchedulePattern(byte frequency = 1, byte offset = 0)
        {
            Frequency = frequency;
            if (frequency >= 1)
            {
                Offset = (byte)(offset % frequency);
            }
        }

        internal bool IsFrameIncluded(int frameCount)
        {
            if (Frequency == 0)
                return false;

            return frameCount % Frequency == Offset;
        }
    }
}
