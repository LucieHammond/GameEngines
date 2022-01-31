using GameEngine.Core.System;
using GameEngine.PMR.Process.Transitions;
using System;

namespace GameEnginesTest.Tools.Mocks.Spies
{
    public class SpyTransition : Transition
    {
        public int PrepareCallCount { get; private set; }
        public int EnterCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }
        public int ExitCallCount { get; private set; }
        public int CleanupCallCount { get; private set; }

        public Action OnPrepare;
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;
        public Action OnCleanup;

        public Configuration ModuleConfiguration => m_ModuleConfiguration;

        public float LoadingProgress => m_LoadingProgress;

        public string LoadingAction => m_LoadingAction;

        public override bool UpdateDuringEntry => true;

        public override bool UpdateDuringExit => true;

        public SpyTransition() : base()
        {
            ResetCount();
        }

        public void ResetCount()
        {
            PrepareCallCount = 0;
            EnterCallCount = 0;
            UpdateCallCount = 0;
            ExitCallCount = 0;
            CleanupCallCount = 0;
        }

        public void SetAutomaticCompletion()
        {
            OnPrepare += MarkReady;
            OnEnter += MarkEntered;
            OnExit += MarkExited;
            OnUpdate += () => { if (!IsComplete) MarkCompleted(); };
        }

        public void CallMarkReady()
        {
            MarkReady();
        }

        public void CallMarkEntered()
        {
            MarkEntered();
        }

        public void CallMarkExited()
        {
            MarkExited();
        }

        public void CallMarkCompleted()
        {
            MarkCompleted();
        }

        protected override void Prepare()
        {
            PrepareCallCount++;
            OnPrepare?.Invoke();
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
