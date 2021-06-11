using GameEnginesTest.Tools.Mocks.Spies;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Utils
{
    public static class FSMUtils
    {
        public static List<SpyFSMState> GetMockStateCollection()
        {
            return new List<SpyFSMState>()
            {
                new SpyFSMState(StatesEnumTest.FirstState),
                new SpyFSMState(StatesEnumTest.SecondState),
                new SpyFSMState(StatesEnumTest.ThirdState),
                new SpyFSMState(StatesEnumTest.FourthState)
            };
        }
    }
}
