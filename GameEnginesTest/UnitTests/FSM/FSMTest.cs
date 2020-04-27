using GameEngine.FSM;
using GameEnginesTest.Tools.Dummy;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GameEnginesTest.UnitTests.FSM
{
    /// <summary>
    /// Unit tests for the class FSM
    /// <see cref="FSM{T}"/>
    /// </summary>
    [TestClass]
    public class FSMTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // 1) Valid args -> fsm is created
            string name = "testFsm";
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection(); // returns a collection of 4 Mock States
            StatesEnumTest initialStateId = StatesEnumTest.FirstState;
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>(name, states, initialStateId);

            // Names are coherent
            Assert.AreEqual(name, fsm.Name);
            foreach (DummyFSMState state in states)
            {
                Assert.AreEqual($"{name}_{state.Id}", state.Name);
            }

            // All states are valid
            foreach (DummyFSMState state in states)
            {
                Assert.IsTrue(fsm.IsValidState(state.Id));
            }

            // CurrentState is the initial state
            Assert.AreEqual(initialStateId, fsm.CurrentStateId);
            Assert.AreEqual(states[0], fsm.CurrentState);

            // 2) Multiple states with same id -> throws ArgumentException
            List<DummyFSMState> invalidStates = new List<DummyFSMState>()
            {
                new DummyFSMState(StatesEnumTest.FirstState),
                new DummyFSMState(StatesEnumTest.SecondState),
                new DummyFSMState(StatesEnumTest.SecondState)
            };
            Assert.ThrowsException<ArgumentException>(() => new FSM<StatesEnumTest>(name, invalidStates, initialStateId));

            // 3) Invalid initial state -> throws ArgumentException
            Assert.ThrowsException<ArgumentException>(() => new FSM<StatesEnumTest>(name, states, StatesEnumTest.FifthState));
        }

        [TestMethod]
        public void StartTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            DummyFSMState initialState = states[0];
            DummyFSMState otherState = states[1];

            // Before start, none of the states methods have been called and the CurrentStateDuration is 0
            Assert.AreEqual(0, initialState.InitializeCallCount);
            Assert.AreEqual(0, initialState.UnloadCallCount);
            Assert.AreEqual(0, initialState.EnterCallCount);
            Assert.AreEqual(0, initialState.UpdateCallCount);
            Assert.AreEqual(0, initialState.ExitCallCount);

            Assert.AreEqual(0, otherState.InitializeCallCount);
            Assert.AreEqual(0, otherState.UnloadCallCount);
            Assert.AreEqual(0, otherState.EnterCallCount);
            Assert.AreEqual(0, otherState.UpdateCallCount);
            Assert.AreEqual(0, otherState.ExitCallCount);

            Assert.AreEqual(0, fsm.CurrentStateDuration);

            Stopwatch watch = Stopwatch.StartNew();
            fsm.Start();

            // After start, the FSM called the Initialize method of all states and the Enter method of the initial state (once)
            Assert.AreEqual(1, initialState.InitializeCallCount);
            Assert.AreEqual(0, initialState.UnloadCallCount);
            Assert.AreEqual(1, initialState.EnterCallCount);
            Assert.AreEqual(0, initialState.UpdateCallCount);
            Assert.AreEqual(0, initialState.ExitCallCount);

            Assert.AreEqual(1, otherState.InitializeCallCount);
            Assert.AreEqual(0, otherState.UnloadCallCount);
            Assert.AreEqual(0, otherState.EnterCallCount);
            Assert.AreEqual(0, otherState.UpdateCallCount);
            Assert.AreEqual(0, otherState.ExitCallCount);

            // The current state is still the initial state
            Assert.AreEqual(StatesEnumTest.FirstState, fsm.CurrentStateId);

            // The current state duration corresponds approximately to the time since start was called
            Thread.Sleep(20);
            Assert.AreEqual(watch.Elapsed.TotalSeconds, fsm.CurrentStateDuration, 0.005);
        }

        [TestMethod]
        public void UpdateTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            DummyFSMState initialState = states[0];

            fsm.Start();

            // Before update, the initial state has been initialized and entered
            Assert.AreEqual(1, initialState.InitializeCallCount);
            Assert.AreEqual(0, initialState.UnloadCallCount);
            Assert.AreEqual(1, initialState.EnterCallCount);
            Assert.AreEqual(0, initialState.UpdateCallCount);
            Assert.AreEqual(0, initialState.ExitCallCount);

            int nbUpdates = 3;
            for (int i = 0; i < nbUpdates; i++)
                fsm.Update();

            // The FSM should have called nbUpdates times the Update method of the initial state
            Assert.AreEqual(1, initialState.InitializeCallCount);
            Assert.AreEqual(0, initialState.UnloadCallCount);
            Assert.AreEqual(1, initialState.EnterCallCount);
            Assert.AreEqual(nbUpdates, initialState.UpdateCallCount);
            Assert.AreEqual(0, initialState.ExitCallCount);

            // The current state is still the initial state
            Assert.AreEqual(StatesEnumTest.FirstState, fsm.CurrentStateId);
        }

        [TestMethod]
        public void StopTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            DummyFSMState initialState = states[0];
            DummyFSMState otherState = states[1];

            Stopwatch watch = Stopwatch.StartNew();
            fsm.Start();
            fsm.Update();

            // Before stop, the initial state has been initialized, entered and updated while the other states have been just initialized
            Assert.AreEqual(1, initialState.InitializeCallCount);
            Assert.AreEqual(0, initialState.UnloadCallCount);
            Assert.AreEqual(1, initialState.EnterCallCount);
            Assert.AreEqual(1, initialState.UpdateCallCount);
            Assert.AreEqual(0, initialState.ExitCallCount);

            Assert.AreEqual(1, otherState.InitializeCallCount);
            Assert.AreEqual(0, otherState.UnloadCallCount);
            Assert.AreEqual(0, otherState.EnterCallCount);
            Assert.AreEqual(0, otherState.UpdateCallCount);
            Assert.AreEqual(0, otherState.ExitCallCount);

            Thread.Sleep(15);
            watch.Stop();
            fsm.Stop();

            // After stop, the FSM called the Exit method of the initial state and the Unload method of all states
            Assert.AreEqual(1, initialState.InitializeCallCount);
            Assert.AreEqual(1, initialState.UnloadCallCount);
            Assert.AreEqual(1, initialState.EnterCallCount);
            Assert.AreEqual(1, initialState.UpdateCallCount);
            Assert.AreEqual(1, initialState.ExitCallCount);

            Assert.AreEqual(1, otherState.InitializeCallCount);
            Assert.AreEqual(1, otherState.UnloadCallCount);
            Assert.AreEqual(0, otherState.EnterCallCount);
            Assert.AreEqual(0, otherState.UpdateCallCount);
            Assert.AreEqual(0, otherState.ExitCallCount);

            // The current state duration corresponds approximately to the time elapsed between start and stop
            Thread.Sleep(10);
            Assert.AreEqual(watch.Elapsed.TotalSeconds, fsm.CurrentStateDuration, 0.005);
        }

        [TestMethod]
        public void SetStateTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            DummyFSMState firstState = states[0];
            DummyFSMState secondState = states[1];
            DummyFSMState thirdState = states[2];
            fsm.Start();

            // CurrentState = first state
            Assert.AreEqual(firstState, fsm.CurrentState);

            // Immediate = false (default) -> the change occurs after update
            fsm.SetState(StatesEnumTest.ThirdState);
            Assert.AreEqual(firstState, fsm.CurrentState);
            fsm.Update();
            Assert.AreEqual(thirdState, fsm.CurrentState);

            // Immediate = true -> the change occurs instantly
            fsm.SetState(StatesEnumTest.SecondState, true);
            Assert.AreEqual(secondState, fsm.CurrentState);

            // IgnoreIfCurrentState = false (default) -> the FSM exit and enter the same state again
            fsm.SetState(StatesEnumTest.SecondState);
            fsm.Update();
            Assert.AreEqual(2, secondState.EnterCallCount);
            Assert.AreEqual(0, secondState.UpdateCallCount);
            Assert.AreEqual(1, secondState.ExitCallCount);

            // IgnoreIfCurrentState = true -> nothing happens when trying to set same state
            fsm.SetState(StatesEnumTest.SecondState, false, true);
            fsm.Update();
            Assert.AreEqual(2, secondState.EnterCallCount);
            Assert.AreEqual(1, secondState.UpdateCallCount);
            Assert.AreEqual(1, secondState.ExitCallCount);

            // Simultaneous change with same priority (default) -> throws InvalidOperationException
            fsm.SetState(StatesEnumTest.FirstState);
            Assert.ThrowsException<InvalidOperationException>(() => fsm.SetState(StatesEnumTest.ThirdState));

            // Simultaneous change with different priorities -> higher priority wins
            fsm.SetState(StatesEnumTest.ThirdState, false, false, 200);
            fsm.SetState(StatesEnumTest.FirstState, false, false, 1);
            fsm.Update();
            Assert.AreEqual(thirdState, fsm.CurrentState);

            // Invalid state -> Throws exception
            Assert.ThrowsException<ArgumentException>(() => fsm.SetState(StatesEnumTest.FifthState));

            // SetState can be called from the Enter, Update or Exit of FSM states
            thirdState.OnUpdate += () => fsm.SetState(StatesEnumTest.SecondState);
            thirdState.OnExit += () => fsm.SetState(StatesEnumTest.ThirdState, priority: 20);
            thirdState.OnEnter += () => fsm.SetState(StatesEnumTest.FirstState);
            fsm.Update();
            Assert.AreEqual(thirdState, fsm.CurrentState);
            thirdState.OnExit = null;
            fsm.Update();
            Assert.AreEqual(firstState, fsm.CurrentState);

            // Check Enter, Update and Exit methods have been called the right number of times
            // First -> Third -> Second -> Second -> Third -> Tird -> First
            Assert.AreEqual(2, firstState.EnterCallCount);
            Assert.AreEqual(0, firstState.UpdateCallCount);
            Assert.AreEqual(1, firstState.ExitCallCount);

            Assert.AreEqual(2, secondState.EnterCallCount);
            Assert.AreEqual(1, secondState.UpdateCallCount);
            Assert.AreEqual(2, secondState.ExitCallCount);

            Assert.AreEqual(3, thirdState.EnterCallCount);
            Assert.AreEqual(1, thirdState.UpdateCallCount);
            Assert.AreEqual(3, thirdState.ExitCallCount);
        }

        [TestMethod]
        public void ResetTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);

            fsm.Start();
            fsm.SetState(StatesEnumTest.SecondState, true);
            fsm.Stop();

            // Before reset, current state is second state
            Assert.AreEqual(StatesEnumTest.SecondState, fsm.CurrentStateId);

            fsm.Reset(StatesEnumTest.FourthState);

            // After reset, current state is fourth state
            Assert.AreEqual(StatesEnumTest.FourthState, fsm.CurrentStateId);

            // fourth state hasn't been entered yet
            Assert.AreEqual(0, states[3].EnterCallCount);
        }

        [TestMethod]
        public void AddStateTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            DummyFSMState fifthState = new DummyFSMState(StatesEnumTest.FifthState);
            fsm.Start();

            // Before AddState, the fifth state is invalid (GetMockStateCollection() returns a collection of four states)
            Assert.IsFalse(fsm.IsValidState(fifthState.Id));

            fsm.AddState(fifthState);

            // After AddState, the fifth state is valid
            Assert.IsTrue(fsm.IsValidState(fifthState.Id));

            // The new state has been initializaed and can be entered
            Assert.AreEqual(1, fifthState.InitializeCallCount);
            Assert.AreEqual(0, fifthState.EnterCallCount);
            fsm.SetState(StatesEnumTest.FifthState, true);
            Assert.AreEqual(1, fifthState.EnterCallCount);

            // Trying to add an already existing state -> throws ArgumentException
            Assert.ThrowsException<ArgumentException>(() => fsm.AddState(states[2]));
        }

        [TestMethod]
        public void RemoveStateTest()
        {
            List<DummyFSMState> states = FSMUtils.GetMockStateCollection();
            FSM<StatesEnumTest> fsm = new FSM<StatesEnumTest>("TestFSM", states, StatesEnumTest.FirstState);
            fsm.Start();

            // Before RemoveState, the second state is valid
            Assert.IsTrue(fsm.IsValidState(StatesEnumTest.SecondState));

            fsm.RemoveState(StatesEnumTest.SecondState);

            // After RemoveState, the second state is invalid
            Assert.IsFalse(fsm.IsValidState(StatesEnumTest.SecondState));

            // The second state has been unloaded and can no longer be entered
            Assert.AreEqual(1, states[1].UnloadCallCount);
            Assert.ThrowsException<ArgumentException>(() => fsm.SetState(StatesEnumTest.SecondState));

            // Trying to remove an unexistant state -> throws ArgumentException
            Assert.ThrowsException<ArgumentException>(() => fsm.RemoveState(StatesEnumTest.FifthState));
        }
    }
}
