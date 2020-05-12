using GameEngine.PJR.Jobs;
using GameEngine.PJR.Process;
using GameEnginesTest.Tools.Mocks;
using System;

namespace GameEnginesTest.Tools.Utils
{
    public static class PJRUtils
    {
        public static bool SimulateExecutionUntil(this GameProcess process, Func<bool> condition, int maxFrames = 10)
        {
            int i = 0;
            while (!condition() && i < maxFrames)
            {
                process.Update();
                ((MockProcessTime) process.Time).GoToNextFrame();
                i++;
            }

            return condition();
        }

        public static bool SimulateExecutionUntil(this GameJob job, MockProcessTime time, Func<bool> condition, int maxFrames = 10)
        {
            int i = 0;
            while (!condition() && i < maxFrames)
            {
                job.Update();
                time.GoToNextFrame();
                i++;
            }

            return condition();
        }
    }
}
