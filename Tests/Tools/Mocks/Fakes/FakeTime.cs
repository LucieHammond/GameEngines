using GameEngine.Core.System;

namespace GameEnginesTest.Tools.Mocks.Fakes
{
    public class FakeTime : ITime
    {
        public float DeltaTime => 16;

        public float Time => FrameCount * DeltaTime;

        public int FrameCount { get; private set; }

        public float RealtimeSinceStartup => Time;

        public FakeTime()
        {
            FrameCount = 0;
        }

        public void GoToNextFrame()
        {
            FrameCount++;
        }
    }
}
