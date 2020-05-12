using GameEngine.PJR.Process;

namespace GameEnginesTest.Tools.Mocks
{
    public class MockProcessTime : IProcessTime
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
