using GameEnginesTest.Mocks;
using System.Collections.Generic;

namespace GameEnginesTest.Utils
{
    public static class FSMUtils
    {
        public static List<MockFSMState> GetMockStateCollection()
        {
            return new List<MockFSMState>()
            {
                new MockFSMState(StatesEnumTest.FirstState),
                new MockFSMState(StatesEnumTest.SecondState),
                new MockFSMState(StatesEnumTest.ThirdState),
                new MockFSMState(StatesEnumTest.FourthState)
            };
        }
    }
}
