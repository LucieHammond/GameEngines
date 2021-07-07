using GameEngine.PMR.Process.Transitions;
using System;

namespace GameEnginesTest.Tools.Mocks.Spies
{
    public class SpyTransition : Transition
    {
        public int InitializeCallCount { get; private set; }
        public int EnterCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int ExitCallCount { get; private set; }
        public int CleanupCallCount { get; private set; }

        public Action OnInitialize;
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;
        public Action OnCleanup;

        public float LoadingProgress => m_LoadingProgress;

        public string LoadingAction => m_LoadingAction;

        public bool UseDefaultReport { get => m_UseDefaultReport; set => m_UseDefaultReport = value; }

        public SpyTransition() : base()
        {
            ResetCount();
        }

        public void ResetCount()
        {
            InitializeCallCount = 0;
            EnterCallCount = 0;
            UpdateCallCount = 0;
            ExitCallCount = 0;
            CleanupCallCount = 0;
        }

        public void SetAutomaticCompletion()
        {
            OnEnter += MarkActivated;
            OnExit += MarkDeactivated;
        }

        public void CallMarkActivated()
        {
            MarkActivated();
        }

        public void CallMarkDeactivated()
        {
            MarkDeactivated();
        }

        protected override void Initialize()
        {
            InitializeCallCount++;
            OnInitialize?.Invoke();
        }

        protected override void Enter()
        {
            EnterCallCount++;
            OnEnter?.Invoke();
        }

        protected override void Update()
        {
            UpdateCallCount++;
            OnUpdate?.Invoke();
        }

        protected override void Exit()
        {
            ExitCallCount++;
            OnExit?.Invoke();
        }

        protected override void Cleanup()
        {
            CleanupCallCount++;
            OnCleanup?.Invoke();
        }
    }
}
