using GameEnginesTest.Tools.Dummy;
using System.Collections.Generic;

namespace GameEnginesTest.Tools.Utils
{
    public static class FSMUtils
    {
        public static List<DummyFSMState> GetMockStateCollection()
        {
            return new List<DummyFSMState>()
            {
                new DummyFSMState(StatesEnumTest.FirstState),
                new DummyFSMState(StatesEnumTest.SecondState),
                new DummyFSMState(StatesEnumTest.ThirdState),
                new DummyFSMState(StatesEnumTest.FourthState)
            };
        }
    }
}
