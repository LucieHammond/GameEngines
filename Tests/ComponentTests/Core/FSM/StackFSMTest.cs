using GameEngine.Core.FSM.CustomFSM;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class StackFSM
    /// <see cref="StackFSM{T}"/>
    /// </summary>
    [TestClass]
    public class StackFSMTest
    {

        [TestMethod]
        public void ManageVerticalStateTransitions()
        {
            // Example : (1) -> 1|(2) -> 1|2|(3) -> 1|2|3|(4) -> 1|2|(3) -> 1|(2) -> (1)

            // Create and start stack FSM with FirstState as initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            StackFSM<StatesEnumTest> stackFsm = new StackFSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            stackFsm.Start();
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            // Push states 2, 3 and 4 on top of each other
            stackFsm.PushState(StatesEnumTest.SecondState, true);
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.CurrentStateId);

            stackFsm.PushState(StatesEnumTest.ThirdState, true);
            Assert.AreEqual(StatesEnumTest.ThirdState, stackFsm.CurrentStateId);

            stackFsm.PushState(StatesEnumTest.FourthState, true);
            Assert.AreEqual(StatesEnumTest.FourthState, stackFsm.CurrentStateId);

            // Pop states one by one to visit them in reverse order
            stackFsm.PopState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, stackFsm.CurrentStateId);

            stackFsm.PopState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.CurrentStateId);

            stackFsm.PopState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            // Stop FSM
            stackFsm.Stop();
        }

        [TestMethod]
        public void ManageHorizontalStateTransitions()
        {
            // Example : (1) -> 1|(2) -> 1 -> 1|(3) -> 1 -> 1|(4) -> (clear) (4)

            // Create and start stack FSM with FirstState as initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            StackFSM<StatesEnumTest> stackFsm = new StackFSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            stackFsm.Start();
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            // Push and Pop each state
            stackFsm.PushState(StatesEnumTest.SecondState, true);
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.CurrentStateId);
            stackFsm.PopState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            stackFsm.PushState(StatesEnumTest.ThirdState, true);
            Assert.AreEqual(StatesEnumTest.ThirdState, stackFsm.CurrentStateId);
            stackFsm.PopState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            stackFsm.PushState(StatesEnumTest.FourthState, true);
            Assert.AreEqual(StatesEnumTest.FourthState, stackFsm.CurrentStateId);

            // Clear stack and check the stack is empty (cannot pop more)
            stackFsm.ClearStateStack();
            Assert.AreEqual(StatesEnumTest.FourthState, stackFsm.CurrentStateId);
            Assert.IsFalse(stackFsm.TryPopState(out StatesEnumTest _));

            // Stop FSM
            stackFsm.Stop();
        }

        [TestMethod]
        public void CanUseSameStateMultipleTimes()
        {
            // Create and start stack FSM with FirstState as initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            StackFSM<StatesEnumTest> stackFsm = new StackFSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            stackFsm.Start();

            // Push multiple times the state 2
            stackFsm.PushState(StatesEnumTest.SecondState, true);
            stackFsm.PushState(StatesEnumTest.FourthState, true);
            stackFsm.PushState(StatesEnumTest.SecondState, true);
            stackFsm.PushState(StatesEnumTest.ThirdState, true);

            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.PopState(true));
            Assert.AreEqual(StatesEnumTest.FourthState, stackFsm.PopState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.PopState(true));

            stackFsm.Stop();
        }

        [TestMethod]
        public void PopFailsWhenStackIsEmpty()
        {
            // Create and start stack FSM with FirstState as initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            StackFSM<StatesEnumTest> stackFsm = new StackFSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            stackFsm.Start();
            Assert.AreEqual(StatesEnumTest.FirstState, stackFsm.CurrentStateId);

            // stack is empty -> PopState will throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => stackFsm.PopState());

            // stack is empty -> TryPopState will return false
            Assert.IsFalse(stackFsm.TryPopState(out StatesEnumTest _));

            stackFsm.Stop();
        }
    }
}
