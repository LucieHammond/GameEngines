using GameEngine.Core.System;

namespace GameEnginesTest.Tools.Mocks
{
    public class MockProcessTime : ITime
    {
        public float DeltaTime => 16;

        public float Time => FrameCount * DeltaTime;

        public int FrameCount { get; private set; }

        public float RealtimeSinceStartup => Time;

        public MockProcessTime()
        {
            FrameCount = 0;
        }

        public void GoToNextFrame()
        {
            FrameCount++;
        }
    }
}
