using GameEngine.PMR.Process;
using GameEngine.PMR.Process.Structure;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubGameProcessSetup : IGameProcessSetup
    {
        public string Name => "TestProcess";

        public IGameServiceSetup CustomServiceSetup;
        public List<IGameModeSetup> CustomGameModes;

        public IGameServiceSetup GetServiceSetup()
        {
            if (CustomServiceSetup != null)
                return CustomServiceSetup;

            return new StubGameServiceSetup();
        }

        public List<IGameModeSetup> GetFirstGameModes()
        {
            if (CustomGameModes != null)
                return CustomGameModes;

            return new List<IGameModeSetup>() { new StubGameModeSetup() };
        }
    }
}
