using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Orchestration;
using GameEnginesTest.Tools.Mocks.Fakes;
using System;

namespace GameEnginesTest.Tools.Utils
{
    public static class SimulationUtils
    {
        #region GameModule

        public static bool SimulateUpToNextState(this GameModule module, FakeTime time, int maxFrames = 10)
        {
            GameModuleState currentState = module.State;
            return SimulateExecutionUntil(module, time, () => module.State != currentState, maxFrames);
        }

        public static bool SimulateExecutionUntil(this GameModule module, FakeTime time, Func<bool> condition, int maxFrames = 10)
        {
            int i = 0;
            while (!condition() && i < maxFrames)
            {
                SimulateOneFrame(module, time);
                i++;
            }

            return condition();
        }

        public static void SimulateOneFrame(this GameModule module, FakeTime time)
        {
            module.Update();
            module.FixedUpdate();
            module.LateUpdate();
            time.GoToNextFrame();
        }

        #endregion

        #region Orchestrator

        internal static bool SimulateUpToNextState(this Orchestrator orchestrator, FakeTime time, int maxFrames = 10)
        {
            OrchestratorState currentState = orchestrator.State;
            return SimulateExecutionUntil(orchestrator, time, () => orchestrator.State != currentState, maxFrames);
        }

        internal static bool SimulateExecutionUntil(this Orchestrator orchestrator, FakeTime time, Func<bool> condition, int maxFrames = 10)
        {
            int i = 0;
            while (!condition() && i < maxFrames)
            {
                SimulateOneFrame(orchestrator, time);
                i++;
            }

            return condition();
        }

        internal static void SimulateOneFrame(this Orchestrator orchestrator, FakeTime time)
        {
            orchestrator.Update();
            orchestrator.FixedUpdate();
            orchestrator.LateUpdate();
            time.GoToNextFrame();
        }

        #endregion

        #region GameProcess

        internal static bool SimulateExecutionUntil(this GameProcess process, FakeTime time, Func<bool> condition, int maxFrames = 10)
        {
            int i = 0;
            while (!condition() && i < maxFrames)
            {
                SimulateOneFrame(process, time);
                i++;
            }

            return condition();
        }

        internal static void SimulateOneFrame(this GameProcess process, FakeTime time)
        {
            process.Update();
            process.FixedUpdate();
            process.LateUpdate();
            time.GoToNextFrame();
        }

        #endregion
    }
}
