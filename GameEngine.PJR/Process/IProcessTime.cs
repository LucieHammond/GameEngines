namespace GameEngine.PJR.Process
{
    /// <summary>
    /// An interface giving time information regarding the GameProcess being run
    /// </summary>
    public interface IProcessTime
    {
        /// <summary>
        /// The completion time in seconds since the last frame update of the GameProcess
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// The time in seconds that elaped between the start of the GameProcess and the beginning of the current frame
        /// </summary>
        float Time { get; }

        /// <summary>
        /// The total number of frames that have passed since the start of the GameProcess
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// The real time in seconds since the GameProcess was started 
        /// </summary>
        float RealtimeSinceStartup { get; }
    }
}
