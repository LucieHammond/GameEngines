using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameEngine.FSM.CustomFSM;
using System.Collections.Generic;
using GameEngine.FSM;
using System;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Utils;

namespace GameEnginesTest.ComponentTests.FSM
{
    /// <summary>
    /// Component tests for the class CycleFSM
    /// <see cref="CycleFSM{T}"/>
    /// </summary>
    [TestClass]
    public class CycleFSMTest
    {
        [TestMethod]
        public void ManageCompleteCycleCourse()
        {
            // If the cycle to go through is composed of all possible states (once each) -> use second simplified constructor
            List<FSMState<StatesEnumTest>> orderedStates = new List<FSMState<StatesEnumTest>>()
            {
                new DummyFSMState(StatesEnumTest.FirstState),
                new DummyFSMState(StatesEnumTest.SecondState),
                new DummyFSMState(StatesEnumTest.ThirdState),
                new DummyFSMState(StatesEnumTest.FourthState)
            };
            CycleFSM<StatesEnumTest> simpleCycleFsm = new CycleFSM<StatesEnumTest>("TestFSM", orderedStates);
            simpleCycleFsm.Start();

            // Visit 1 -> 2 -> 3 -> 4 -> 1 -> 2 ...
            Assert.AreEqual(StatesEnumTest.FirstState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, simpleCycleFsm.CurrentStateId);
            simpleCycleFsm.Stop();

            // For using a custom cycle -> use first constructor
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> cycleOrder = new List<StatesEnumTest>()
            {
                StatesEnumTest.FirstState,
                StatesEnumTest.FourthState,
                StatesEnumTest.ThirdState,
                StatesEnumTest.FourthState,
            };
            CycleFSM<StatesEnumTest> customCycleFsm = new CycleFSM<StatesEnumTest>("TestFSM", states, cycleOrder);
            customCycleFsm.Start();

            // Visit 1 -> 4 -> 3 -> 4 -> 1 -> 4 ...
            Assert.AreEqual(StatesEnumTest.FirstState, customCycleFsm.CurrentStateId);
            customCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, customCycleFsm.CurrentStateId);
            customCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, customCycleFsm.CurrentStateId);
            customCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, customCycleFsm.CurrentStateId);
            customCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, customCycleFsm.CurrentStateId);
            customCycleFsm.MoveToNextState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, customCycleFsm.CurrentStateId);
            customCycleFsm.Stop();
        }

        [TestMethod]
        public void ChangeCycleWhileMovingOnIt()
        {
            // Create and start cycle Fsm
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> cycleOrder = new List<StatesEnumTest>()
            {
                StatesEnumTest.FirstState,
                StatesEnumTest.SecondState
            };
            CycleFSM<StatesEnumTest> cycleFsm = new CycleFSM<StatesEnumTest>("TestFSM", states, cycleOrder);
            cycleFsm.Start();

            // Insert state at index > currentIndex
            cycleFsm.InsertStateInCycle(StatesEnumTest.FourthState, 2);
            Assert.AreEqual(StatesEnumTest.SecondState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.FourthState, cycleFsm.MoveToNextState(true));

            // Insert state at index <= currentIndex
            cycleFsm.InsertStateInCycle(StatesEnumTest.ThirdState, 2);
            Assert.AreEqual(StatesEnumTest.FirstState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.ThirdState, cycleFsm.MoveToNextState(true));

            // Withdraw state at index > currentIndex
            cycleFsm.WithdrawStateFromCycle(3);
            Assert.AreEqual(StatesEnumTest.FirstState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, cycleFsm.MoveToNextState(true));

            // Withdraw state at index < currentIndex
            cycleFsm.WithdrawStateFromCycle(0);
            Assert.AreEqual(StatesEnumTest.ThirdState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, cycleFsm.MoveToNextState(true));

            // Try to withdraw current state -> throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => cycleFsm.WithdrawStateFromCycle(0));

            cycleFsm.Stop();
        }

        [TestMethod]
        public void CanUseSameStateMultipleTimes()
        {
            // Create cycle fsm with twice the state 3
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> cycleOrder = new List<StatesEnumTest>()
            {
                StatesEnumTest.ThirdState,
                StatesEnumTest.ThirdState,
                StatesEnumTest.FourthState,
            };
            CycleFSM<StatesEnumTest> cycleFsm = new CycleFSM<StatesEnumTest>("TestFSM", states, cycleOrder);
            cycleFsm.Start();

            Assert.AreEqual(StatesEnumTest.ThirdState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.FourthState, cycleFsm.MoveToNextState(true));
            Assert.AreEqual(StatesEnumTest.ThirdState, cycleFsm.MoveToNextState(true));

            cycleFsm.Stop();
        }

        [TestMethod]
        public void OperationsFailsWithIncorrectIndices()
        {
            // Create and start cycle Fsm with 4 states and a cycle of length 3
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> cycleOrder = new List<StatesEnumTest>()
            {
                StatesEnumTest.FirstState,
                StatesEnumTest.SecondState,
                StatesEnumTest.ThirdState
            };
            CycleFSM<StatesEnumTest> cycleFsm = new CycleFSM<StatesEnumTest>("TestFSM", states, cycleOrder);
            cycleFsm.Start();

            // Call InsertStateInCycle with index out of range -> throw ArgumentOutOfRangeException
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cycleFsm.InsertStateInCycle(StatesEnumTest.FirstState, -1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cycleFsm.InsertStateInCycle(StatesEnumTest.FirstState, cycleOrder.Count + 1));

            // Call WithdrawStateFromCycle with index out of range -> throw ArgumentOutOfBoundException
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cycleFsm.WithdrawStateFromCycle(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cycleFsm.WithdrawStateFromCycle(cycleOrder.Count));

            cycleFsm.Stop();
        }
    }
}
