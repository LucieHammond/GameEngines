using GameEngine.PMR.Modules;
using GameEngine.PMR.Process;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubGameProcessSetup : IGameProcessSetup
    {
        public string Name => "TestProcess";

        public IGameModuleSetup CustomServiceSetup;
        public List<IGameModuleSetup> CustomGameModes;

        public IGameModuleSetup GetServiceSetup()
        {
            if (CustomServiceSetup != null)
                return CustomServiceSetup;

            return new StubGameServiceSetup();
        }

        public List<IGameModuleSetup> GetFirstGameModes()
        {
            if (CustomGameModes != null)
                return CustomGameModes;

            return new List<IGameModuleSetup>() { new StubGameModeSetup() };
        }
    }
}
