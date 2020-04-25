using GameEngine.PJR.Process;
using GameEngine.PJR.Process.Modes;
using GameEngine.PJR.Process.Services;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameProcessSetup : IGameProcessSetup
    {
        public string Name => "Test";

        public IServiceSetup GetServiceSetup()
        {
            return new DummyServiceSetup();
        }

        public List<IGameModeSetup> GetFirstGameModes()
        {
            return new List<IGameModeSetup>() { new DummyGameModeSetup() };
        }
    }
}
