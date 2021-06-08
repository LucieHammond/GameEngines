using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Structure;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Dummy
{
    public class DummyGameProcessSetup : IGameProcessSetup
    {
        public string Name => "Test";

        public IGameServiceSetup CustomServiceSetup;
        public List<IGameModeSetup> CustomGameModes;

        public IGameServiceSetup GetServiceSetup()
        {
            if (CustomServiceSetup != null)
                return CustomServiceSetup;

            return new DummyGameServiceSetup();
        }

        public List<IGameModeSetup> GetFirstGameModes()
        {
            if (CustomGameModes != null)
                return CustomGameModes;

            return new List<IGameModeSetup>() { new DummyGameModeSetup() };
        }
    }
}
