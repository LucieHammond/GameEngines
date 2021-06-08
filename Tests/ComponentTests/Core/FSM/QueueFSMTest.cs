using GameEngine.Core.FSM;
using GameEngine.Core.FSM.CustomFSM;
using GameEnginesTest.Tools.Mocks.Spies;
using GameEnginesTest.Tools.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GameEnginesTest.ComponentTests.Core
{
    /// <summary>
    /// Component tests for the class QueueFSM
    /// <see cref="QueueFSM{T}"/>
    /// </summary>
    [TestClass]
    public class QueueFSMTest
    {
        [TestMethod]
        public void ManageMovesOnPredefinedPath()
        {
            // If predefined path is an ordered list of all states -> use second simplified constructor
            List<FSMState<StatesEnumTest>> orderedStates = new List<FSMState<StatesEnumTest>>()
            {
                new SpyFSMState(StatesEnumTest.FirstState),
                new SpyFSMState(StatesEnumTest.FourthState),
                new SpyFSMState(StatesEnumTest.ThirdState),
                new SpyFSMState(StatesEnumTest.FifthState),
                new SpyFSMState(StatesEnumTest.SecondState)
            };
            QueueFSM<StatesEnumTest> simpleQueueFsm = new QueueFSM<StatesEnumTest>("TestFSM", orderedStates);
            simpleQueueFsm.Start();

            // Visit 1 -> 4 -> 3 -> 5 -> 2
            Assert.AreEqual(StatesEnumTest.FirstState, simpleQueueFsm.CurrentStateId);
            simpleQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, simpleQueueFsm.CurrentStateId);
            simpleQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, simpleQueueFsm.CurrentStateId);
            simpleQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FifthState, simpleQueueFsm.CurrentStateId);
            simpleQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, simpleQueueFsm.CurrentStateId);

            // Check queue is empty and stop fsm
            Assert.IsFalse(simpleQueueFsm.TryDequeueState(out StatesEnumTest _));
            simpleQueueFsm.Stop();

            // If predefined path is custom -> use first constructor
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> order = new List<StatesEnumTest>()
            {
                StatesEnumTest.FirstState,
                StatesEnumTest.ThirdState,
                StatesEnumTest.SecondState,
                StatesEnumTest.SecondState,
                StatesEnumTest.ThirdState
            };
            QueueFSM<StatesEnumTest> customQueueFsm = new QueueFSM<StatesEnumTest>("TestFSM", states, order);
            customQueueFsm.Start();

            // Visit 1 -> 3 -> 2 -> 2 -> 3
            Assert.AreEqual(StatesEnumTest.FirstState, customQueueFsm.CurrentStateId);
            customQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, customQueueFsm.CurrentStateId);
            customQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, customQueueFsm.CurrentStateId);
            customQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, customQueueFsm.CurrentStateId);
            customQueueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, customQueueFsm.CurrentStateId);

            // Check queue is empty and stop fsm
            Assert.IsFalse(customQueueFsm.TryDequeueState(out StatesEnumTest _));
            customQueueFsm.Stop();
        }

        [TestMethod]
        public void ManageMovesOnLateDefinedPath()
        {
            // Use first constructor to create a queueFsm with only an initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> order = new List<StatesEnumTest>() { StatesEnumTest.FirstState };
            QueueFSM<StatesEnumTest> queueFsm = new QueueFSM<StatesEnumTest>("TestFSM", states, order);
            queueFsm.Start();

            // Check queue is empty
            Assert.IsFalse(queueFsm.TryDequeueState(out StatesEnumTest _));
            Assert.AreEqual(StatesEnumTest.FirstState, queueFsm.CurrentStateId);

            // Enqueue and dequeue each state to visit one after the other
            queueFsm.EnqueueState(StatesEnumTest.FourthState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, queueFsm.CurrentStateId);

            queueFsm.EnqueueState(StatesEnumTest.ThirdState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.ThirdState, queueFsm.CurrentStateId);

            queueFsm.EnqueueState(StatesEnumTest.FirstState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, queueFsm.CurrentStateId);

            queueFsm.EnqueueState(StatesEnumTest.SecondState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.SecondState, queueFsm.CurrentStateId);

            // Enqueue new states and clear queue to redefine the states to visit
            queueFsm.EnqueueState(StatesEnumTest.FirstState);
            queueFsm.EnqueueState(StatesEnumTest.SecondState);
            queueFsm.EnqueueState(StatesEnumTest.ThirdState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FirstState, queueFsm.CurrentStateId);

            queueFsm.ClearStateQueue();
            queueFsm.EnqueueState(StatesEnumTest.FourthState);
            queueFsm.DequeueState(true);
            Assert.AreEqual(StatesEnumTest.FourthState, queueFsm.CurrentStateId);

            // Check queue is empty and stop fsm
            Assert.IsFalse(queueFsm.TryDequeueState(out StatesEnumTest _));
            queueFsm.Stop();
        }

        [TestMethod]
        public void CanUseSameStateMultipleTimes()
        {
            // Create and start queue FSM with SecondState as initial state
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();
            List<StatesEnumTest> order = new List<StatesEnumTest>() { StatesEnumTest.SecondState };
            QueueFSM<StatesEnumTest> stackFsm = new QueueFSM<StatesEnumTest>("TestFSM", states, order);
            stackFsm.Start();

            // Enqueue multiple times the state 2
            stackFsm.EnqueueState(StatesEnumTest.SecondState);
            stackFsm.EnqueueState(StatesEnumTest.SecondState);
            stackFsm.EnqueueState(StatesEnumTest.SecondState);

            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.DequeueState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.DequeueState(true));
            Assert.AreEqual(StatesEnumTest.SecondState, stackFsm.DequeueState(true));

            stackFsm.Stop();
        }

        [TestMethod]
        public void DequeueFailsWhenQueueIsEmpty()
        {
            List<SpyFSMState> states = FSMUtils.GetMockStateCollection();

            // Cannot create queue FSM with no initial state to dequeue -> throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => new QueueFSM<StatesEnumTest>("TestFSM", states, new List<StatesEnumTest>()));
            Assert.ThrowsException<InvalidOperationException>(() => new QueueFSM<StatesEnumTest>("TestFSM", new List<FSMState<StatesEnumTest>>()));

            // Create and start an empty queue FSM with FirstState as initial state
            QueueFSM<StatesEnumTest> queueFsm = new QueueFSM<StatesEnumTest>("TestFSM", states, new List<StatesEnumTest>() { StatesEnumTest.FirstState });
            queueFsm.Start();
            Assert.AreEqual(StatesEnumTest.FirstState, queueFsm.CurrentStateId);

            // queue is empty -> DequeueState will throw InvalidOperationException
            Assert.ThrowsException<InvalidOperationException>(() => queueFsm.DequeueState());

            // queue is empty -> TryDequeueState will return false
            Assert.IsFalse(queueFsm.TryDequeueState(out StatesEnumTest _));

            queueFsm.Stop();
        }
    }
}
