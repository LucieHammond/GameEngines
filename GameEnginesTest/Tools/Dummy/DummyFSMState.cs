using GameEngine.FSM;
using System;

namespace GameEnginesTest.Tools.Dummy
{
    public enum StatesEnumTest
    {
        FirstState,
        SecondState,
        ThirdState,
        FourthState,
        FifthState
    }

    public class DummyFSMState : FSMState<StatesEnumTest>
    {
        public override StatesEnumTest Id => m_StateId;
        private readonly StatesEnumTest m_StateId;

        public int InitializeCallCount { get; private set; }
        public int UnloadCallCount { get; private set; }
        public int EnterCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int ExitCallCount { get; private set; }

        public Action OnInitialize;
        public Action OnUnload;
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;

        public DummyFSMState(StatesEnumTest id)
        {
            m_StateId = id;

            InitializeCallCount = 0;
            UnloadCallCount = 0;
            EnterCallCount = 0;
            UpdateCallCount = 0;
            ExitCallCount = 0;
        }

        public override void Initialize()
        {
            InitializeCallCount++;
            OnInitialize?.Invoke();
        }

        public override void Unload()
        {
            UnloadCallCount++;
            OnUnload?.Invoke();
        }

        public override void Enter()
        {
            EnterCallCount++;
            OnEnter?.Invoke();
        }

        public override void Update()
        {
            UpdateCallCount++;
            OnUpdate?.Invoke();
        }

        public override void Exit()
        {
            ExitCallCount++;
            OnExit?.Invoke();
        }
    }
}
